using System;using System.Collections.Generic;using System.Reflection;using System.Reflection.Emit;using UnityEngine;
namespace RezRocket {
 internal static class FlightRespawnRules {
  internal static bool Suppress(bool enabled,bool active,bool fly,bool rocket,object car,object flyCar,object rocketCar){return enabled&&active&&car!=null&&((fly&&object.ReferenceEquals(car,flyCar))||(rocket&&object.ReferenceEquals(car,rocketCar)));}
 }
 public sealed partial class Exporter {
  bool preventFlightRespawn=true;static Exporter flightRespawnOwner;object flightRespawnHarmony;int flightRespawnBlocks;string flightRespawnStatus="";
  readonly Dictionary<MethodBase,MethodInfo> flightRespawnPatches=new Dictionary<MethodBase,MethodInfo>();
  bool ShouldPreventFlightRespawn(object car){return FlightRespawnRules.Suppress(preventFlightRespawn,isActiveAndEnabled,false,rocketEnabled,car,null,rocketCar);}
  public static void FlightAutomaticReset(object car){Exporter owner=flightRespawnOwner;if(owner!=null&&owner.ShouldPreventFlightRespawn(car)){owner.flightRespawnBlocks++;return;}Call(car,"ResetToSpawnPointOnUgcMap");}
  public static void FlightAutomaticFlip(object car){Exporter owner=flightRespawnOwner;if(owner!=null&&owner.ShouldPreventFlightRespawn(car)){owner.flightRespawnBlocks++;return;}Call(car,"FlipCar");}
  public static IEnumerable<T> FlightRespawnTranspiler<T>(IEnumerable<T> instructions,MethodBase __originalMethod){
   Type i=typeof(T);FieldInfo opcode=i.GetField("opcode"),operand=i.GetField("operand");if(opcode==null||operand==null)throw new InvalidOperationException("Unsupported Harmony instruction format.");
   bool reset=__originalMethod.DeclaringType.Name.StartsWith("<WaitOnGround>",StringComparison.Ordinal);string native=reset?"ResetToSpawnPointOnUgcMap":"FlipCar";
   MethodInfo hook=typeof(Exporter).GetMethod(reset?"FlightAutomaticReset":"FlightAutomaticFlip",BindingFlags.Public|BindingFlags.Static);var result=new List<T>(instructions);int count=0;
   foreach(T instruction in result){MethodInfo method=operand.GetValue(instruction) as MethodInfo;if(method==null||method.DeclaringType.FullName!="RaceCar"||method.Name!=native)continue;OpCode op=(OpCode)opcode.GetValue(instruction);if(op!=OpCodes.Call&&op!=OpCodes.Callvirt)continue;opcode.SetValue(instruction,OpCodes.Call);operand.SetValue(instruction,hook);count++;}
   if(count!=1)throw new InvalidOperationException("Automatic respawn call layout changed; protection was not installed.");return result;
  }
  void EnsureFlightRespawnGuard(){
   if(flightRespawnHarmony!=null){flightRespawnOwner=this;return;}
   BundledHarmony.EnsureLoaded();flightRespawnHarmony=Activator.CreateInstance(FindType("HarmonyLib.Harmony"),new object[]{"RezRocket.flight-auto-respawn"});flightRespawnOwner=this;
   try{
    Type race=FindType("RaceCar"),instruction=flightRespawnHarmony.GetType().Assembly.GetType("HarmonyLib.CodeInstruction");
    if(race==null||instruction==null)throw new InvalidOperationException("Native automatic respawn hooks unavailable.");
    MethodInfo hook=typeof(Exporter).GetMethod("FlightRespawnTranspiler").MakeGenericMethod(instruction);
    foreach(string prefix in new[]{"<WaitOnGround>","<WaitOnFlip>"}){
     Type machine=null;foreach(Type nested in race.GetNestedTypes(Flags))if(nested.Name.StartsWith(prefix,StringComparison.Ordinal)){if(machine!=null)throw new InvalidOperationException("Ambiguous automatic respawn routine.");machine=nested;}
     MethodInfo original=machine==null?null:machine.GetMethod("MoveNext",Flags);if(original==null)throw new MissingMethodException("Automatic respawn routine unavailable.");
     object descriptor=Activator.CreateInstance(FindType("HarmonyLib.HarmonyMethod"),new object[]{hook});bool patched=false;
     flightRespawnPatches[original]=hook;
     foreach(MethodInfo patch in flightRespawnHarmony.GetType().GetMethods())if(patch.Name=="Patch"){var p=patch.GetParameters();int slot=Array.FindIndex(p,x=>x.Name=="transpiler");if(slot<0)continue;object[] args=new object[p.Length];args[0]=original;args[slot]=descriptor;patch.Invoke(flightRespawnHarmony,args);patched=true;break;}
     if(!patched)throw new MissingMethodException("Harmony automatic respawn hook unavailable.");
    }
    flightRespawnStatus="Automatic airborne reset/flip protection ready";
   }catch{ReleaseFlightRespawnGuard();throw;}
  }
  void ReleaseFlightRespawnGuard(){
   if(flightRespawnOwner==this)flightRespawnOwner=null;
   if(flightRespawnHarmony!=null){MethodInfo undo=flightRespawnHarmony.GetType().GetMethod("Unpatch",new[]{typeof(MethodBase),typeof(MethodInfo)});foreach(var pair in flightRespawnPatches)try{if(undo!=null)undo.Invoke(flightRespawnHarmony,new object[]{pair.Key,pair.Value});}catch(Exception e){Debug.LogWarning("RCLI flight guard cleanup: "+e.Message);}}
   flightRespawnPatches.Clear();flightRespawnHarmony=null;
  }
  void DrawFlightRespawnOption(){
   preventFlightRespawn=GUILayout.Toggle(preventFlightRespawn," Prevent automatic respawn while flying");
   GUILayout.Label("Blocks your car's Workshop airborne reset and auto-flip only while Rocket or Fly Mode is enabled. Manual reset remains available.",smallStyle);
   if(preventFlightRespawn&&!string.IsNullOrEmpty(flightRespawnStatus))GUILayout.Label(flightRespawnStatus+" · blocked "+flightRespawnBlocks,smallStyle);
  }
 }
}
