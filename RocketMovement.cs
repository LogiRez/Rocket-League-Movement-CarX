using System;using System.Reflection;using UnityEngine;
namespace RezRocket {
 internal static class RocketMovementRules {
  internal static bool CanJump(bool grounded,bool airJumps,float now,float last,float cooldown){return (grounded||airJumps)&&now-last>=cooldown;}
  internal static bool CanThrust(float forwardSpeed,float direction,float limit){return direction>0?forwardSpeed<limit:direction<0?forwardSpeed>-limit:false;}
  internal static float Deadzone(float value){return Math.Abs(value)<.12f?0:Math.Max(-1,Math.Min(1,value));}
 }
 public sealed partial class Exporter {
  bool rocketEnabled,rocketAirJumps=true,rocketStabilize=true,rocketJumpQueued;
  int rocketRequest;float rocketThrust=20,rocketReverse=12,rocketJump=7,rocketTurn=110,rocketMaxSpeed=350,rocketLastJump=-100,rocketJumpExpires;
  Vector3 rocketSteer;float rocketForward,rocketBackward;bool rocketBoost;
  Component rocketCar;Rigidbody rocketBody;object rocketState;Type rocketGamepadType;PropertyInfo rocketPadCurrent;MethodInfo rocketInputLock;
  string rocketStatus="Off";
  object RocketState(){Type states=FindType("States");return states==null?null:states.GetProperty("mainCurrent",Flags).GetValue(null,null);}
  Component RocketCurrentCar(object state){return R(state,"userCar") as Component;}
  void ClearRocketInput(){rocketForward=rocketBackward=0;rocketSteer=Vector3.zero;rocketBoost=false;rocketJumpQueued=false;}
  void StopRocketMovement(string reason="Off"){ReleaseMotionLease();rocketEnabled=false;rocketCar=null;rocketBody=null;rocketState=null;rocketRequest=0;ClearRocketInput();rocketStatus=reason;}
  bool RocketConflict(){return false;}
  void StartRocketMovement(){
   if(RocketConflict())throw new InvalidOperationException("Stop Fly, Attachment, Race/Brick Wall, Automation and active car operations first.");
   object state=RocketState();Type race=FindType("BaseRaceState");Component car=RocketCurrentCar(state);
   if(state==null||race==null||!race.IsInstanceOfType(state)||car==null||object.Equals(R(car,"m_inGarage"),true)||!object.Equals(R(car,"isNetworkCar"),false)||!object.Equals(R(car,"IsVisualLoaded"),true))throw new InvalidOperationException("Spawn your local car outside the garage and wait for it to load.");
   Rigidbody body=R(car,"getRigidbody") as Rigidbody;if(body==null||body.isKinematic)throw new InvalidOperationException("Car physics is not ready.");
   if(!(R(car,"onGround") is bool))throw new InvalidOperationException("Ground-contact information is unavailable.");
   rocketState=state;rocketCar=car;rocketBody=body;ClearRocketInput();rocketLastJump=-100;
   rocketGamepadType=FindType("UnityEngine.InputSystem.Gamepad");rocketPadCurrent=rocketGamepadType==null?null:rocketGamepadType.GetProperty("current",Flags);
   Type manager=FindType("GameInput.InputManager");rocketInputLock=manager==null?null:manager.GetMethod("GetLockedInputObject",Flags,null,Type.EmptyTypes,null);
   EnsureFlightRespawnGuard();AcquireMotionLease();rocketEnabled=true;rocketStatus="Ready · close the menu to drive";
  }
  bool RocketPaused(){return visible||menuInputCaptured||!Application.isFocused||(rocketInputLock!=null&&rocketInputLock.Invoke(null,null)!=null);}
  float RocketButton(object pad,string name){object control=R(pad,name);return control==null?0:Mathf.Clamp01(Convert.ToSingle(Call(control,"ReadValue")));}
  Vector2 RocketStick(object pad,string name){object control=R(pad,name);if(control==null)return Vector2.zero;return (Vector2)Call(control,"ReadValue");}
  void UpdateRocketMovement(){
   try{
    if(rocketRequest!=0){int requested=rocketRequest;rocketRequest=0;if(requested<0)StopRocketMovement();else StartRocketMovement();}
    if(!rocketEnabled)return;
    if(RocketConflict()){StopRocketMovement("Stopped: another movement/car operation started");return;}
    object state=RocketState();if(!object.ReferenceEquals(state,rocketState)||rocketCar==null||rocketBody==null||!object.ReferenceEquals(RocketCurrentCar(state),rocketCar)||!rocketCar.gameObject.activeInHierarchy||object.Equals(R(rocketCar,"m_inGarage"),true)){StopRocketMovement("Stopped: car or session changed");return;}
    if(RocketPaused()){ClearRocketInput();rocketStatus="Paused while menu/chat is open or game is unfocused";return;}
    // Reset continuous inputs before reading the current pad: disconnecting a
    // controller cannot leave a cached trigger or boost held down.
    rocketForward=rocketBackward=0;rocketSteer=Vector3.zero;rocketBoost=false;
    if(rocketJumpQueued&&Time.unscaledTime>rocketJumpExpires)rocketJumpQueued=false;
    if(Input.GetKey(KeyCode.LeftShift))rocketForward=1;if(Input.GetKey(KeyCode.LeftAlt))rocketBackward=1;
    float pitch=(Input.GetKey(KeyCode.DownArrow)?1:0)-(Input.GetKey(KeyCode.UpArrow)?1:0),yaw=(Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.LeftArrow)?1:0),roll=(Input.GetKey(KeyCode.Q)?1:0)-(Input.GetKey(KeyCode.E)?1:0);
    bool jump=Input.GetKeyDown(KeyCode.Space);object pad=rocketPadCurrent==null?null:rocketPadCurrent.GetValue(null,null);
    if(pad!=null){Vector2 left=RocketStick(pad,"leftStick"),right=RocketStick(pad,"rightStick");pitch+=RocketMovementRules.Deadzone(-left.y);yaw+=RocketMovementRules.Deadzone(left.x);roll+=RocketMovementRules.Deadzone(-right.x);rocketForward=Mathf.Max(rocketForward,RocketButton(pad,"rightTrigger"));rocketBackward=Mathf.Max(rocketBackward,RocketButton(pad,"leftTrigger"));rocketBoost=RocketButton(pad,"leftShoulder")>.1f;object west=R(pad,"buttonWest");jump=jump||object.Equals(R(west,"wasPressedThisFrame"),true);}
    rocketSteer=new Vector3(Mathf.Clamp(pitch,-1,1),Mathf.Clamp(yaw,-1,1),Mathf.Clamp(roll,-1,1));
    if(jump){rocketJumpQueued=true;rocketJumpExpires=Time.unscaledTime+.2f;}
    rocketStatus="Active · "+(rocketBody.velocity.magnitude*3.6f).ToString("0")+" km/h";
   }catch(Exception e){StopRocketMovement("Stopped: "+e.GetBaseException().Message);}
  }
  void FixedRocketMovement(){
   if(!rocketEnabled)return;
   try{
    if(RocketConflict()||RocketPaused()){ClearRocketInput();return;}
    if(rocketCar==null||rocketBody==null||rocketBody.isKinematic||!object.ReferenceEquals(RocketState(),rocketState)||!object.ReferenceEquals(RocketCurrentCar(rocketState),rocketCar)){StopRocketMovement("Stopped: car changed");return;}
    bool grounded=object.Equals(R(rocketCar,"onGround"),true);
    float thrust=rocketForward*rocketThrust-rocketBackward*rocketReverse;if(rocketBoost)thrust*=2;
    Vector3 forward=rocketBody.rotation*Vector3.forward;float speed=Vector3.Dot(rocketBody.velocity,forward);
    if(RocketMovementRules.CanThrust(speed,thrust,rocketMaxSpeed/3.6f))rocketBody.AddForce(forward*thrust,ForceMode.Acceleration);
    if(!grounded&&(rocketStabilize||rocketSteer.sqrMagnitude>.001f)){
     Vector3 localAngular=Quaternion.Inverse(rocketBody.rotation)*rocketBody.angularVelocity;
     Vector3 target=rocketSteer*(rocketTurn*Mathf.Deg2Rad);
     Vector3 torque=Vector3.ClampMagnitude((target-localAngular)*6,24);
     rocketBody.AddRelativeTorque(torque,ForceMode.Acceleration);
    }
    if(rocketJumpQueued){bool fresh=Time.unscaledTime<=rocketJumpExpires;rocketJumpQueued=false;if(fresh&&RocketMovementRules.CanJump(grounded,rocketAirJumps,Time.unscaledTime,rocketLastJump,.35f)){rocketLastJump=Time.unscaledTime;Vector3 v=rocketBody.velocity;if(v.y<0){v.y=0;rocketBody.velocity=v;}rocketBody.AddForce(Vector3.up*rocketJump,ForceMode.VelocityChange);}}
   }catch(Exception e){StopRocketMovement("Stopped: "+e.GetBaseException().Message);}
  }
  void DrawRocketMovement(){
   GUILayout.BeginVertical(cardStyle);GUILayout.Label("Rocket movement",nameStyle);
   bool enabled=GUILayout.Toggle(rocketEnabled," Enable rocket movement");if(enabled!=rocketEnabled)rocketRequest=enabled?1:-1;
   GUILayout.Label("Boost, jump and steer in the air with normal gravity and collisions. Air steering applies only when the wheels are off the ground.",smallStyle);
   GUILayout.Label("Forward thrust: "+rocketThrust.ToString("0")+" m/s²",smallStyle);rocketThrust=GUILayout.HorizontalSlider(rocketThrust,5,60);
   GUILayout.Label("Reverse thrust: "+rocketReverse.ToString("0")+" m/s²",smallStyle);rocketReverse=GUILayout.HorizontalSlider(rocketReverse,5,40);
   GUILayout.Label("Jump strength: "+rocketJump.ToString("0.0")+" m/s",smallStyle);rocketJump=GUILayout.HorizontalSlider(rocketJump,2,15);
   GUILayout.Label("Air turn speed: "+rocketTurn.ToString("0")+"°/s",smallStyle);rocketTurn=GUILayout.HorizontalSlider(rocketTurn,30,240);
   GUILayout.Label("Boost speed limit: "+rocketMaxSpeed.ToString("0")+" km/h",smallStyle);rocketMaxSpeed=GUILayout.HorizontalSlider(rocketMaxSpeed,80,500);
   DrawFlightRespawnOption();
   rocketAirJumps=GUILayout.Toggle(rocketAirJumps," Allow mid-air jumps");rocketStabilize=GUILayout.Toggle(rocketStabilize," Stabilize air rotation when the stick is released");
   if(ActionButton("Reset rocket settings",GUILayout.Width(260))){rocketThrust=20;rocketReverse=12;rocketJump=7;rocketTurn=110;rocketMaxSpeed=350;rocketAirJumps=rocketStabilize=true;}
   GUILayout.Label(rocketStatus,smallStyle);GUILayout.EndVertical();
   GUILayout.BeginVertical(cardStyle);GUILayout.Label("Controls",nameStyle);
   GUILayout.Label("Controller: RT forward · LT reverse · LB double thrust · X / Square jump · left stick air pitch/yaw · right stick left/right air roll.\nKeyboard: Left Shift forward · Left Alt reverse · Space jump · arrow keys air pitch/yaw · Q/E air roll.\nF10 stops rocket movement. Normal driving controls remain active. Pauses for menu/input locks and focus loss. Stops on car/session changes or conflicting movement modes. Changes to strength settings apply immediately; the mode starts off each session.",smallStyle);
   GUILayout.Label("Standalone tool by rez. Independent of RCLI, RL.dll and BepInEx. Local movement uses CarX's normal replication; multiplayer appearance and handling still need in-game testing.",smallStyle);GUILayout.EndVertical();
  }
 }
}
