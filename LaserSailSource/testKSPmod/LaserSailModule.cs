using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LaserSail
{
    public class ModuleLaserSail : PartModule
    {
        //Sail Data
        [KSPField(isPersistant = true)]
        public bool isDeployed = false;
        [KSPField(isPersistant = false)]
        public bool isOn = false;
        [KSPField(isPersistant = true)]
        public double secBought;

        [KSPField(guiName = "Laser Seconds to Buy", guiFormat = "F1", guiActive = true, isPersistant = false, guiActiveEditor = false)]
        [UI_FloatRange(minValue = 0f, maxValue = 10f, stepIncrement = 0.1f, controlEnabled = true)]
        public float timeToBuy = 0;

        [KSPField]
        public float laserForce;
        [KSPField]
        public String animName;

        public float laserCost = 15000;
        public double laserAccD = 0;
        public double laserForceD = 0;
        public double dtk = 0;

        //Interface text
        [KSPField(guiActive = true, guiName = "Seconds avaliable")]
        protected string secBoughtS = "";
        [KSPField(guiActive = true, guiName = "Cost per second")]
        protected string laserCostS = "";
        [KSPField(guiActive = true, guiName = "Acceleration")]
        protected string laserAccS = "";
        [KSPField(guiActive = true, guiName = "Status")]
        protected string status = "";

        private Animation deployAnimation;


        //Buy Seconds to be used later
        [KSPEvent(guiActive = true, guiName = "Buy Seconds", active = true)]
        public void buySailSeconds()
        {
            if(timeToBuy > 0)
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    if (Funding.CanAfford(timeToBuy * laserCost))
                    {
                        secBought += timeToBuy;
                        print("[Sprite Mod] Bought " + timeToBuy + " seconds for " + (laserCost * timeToBuy) + " funds");
                        ScreenMessages.PostScreenMessage("-" + (laserCost * timeToBuy) + " funds", 2.0f, ScreenMessageStyle.UPPER_CENTER);
                        Funding.Instance.AddFunds(-(laserCost * timeToBuy), TransactionReasons.None);
                        timeToBuy = 0;
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage("Not enough money", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                else
                {
                    secBought += timeToBuy;
                    print("[Sprite Mod] Bought " + timeToBuy + " seconds");
                    timeToBuy = 0;
                }
            }
        }

        [KSPEvent(guiActive = true, guiName = "Sell Seconds", active = true)]
        public void sellSailSeconds()
        {
            if(secBought > 0.001)
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    ScreenMessages.PostScreenMessage("Sold " + secBought + " seconds for " + (secBought * laserCost), 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    Funding.Instance.AddFunds((laserCost * secBought), TransactionReasons.None);
                    secBought = 0;
                }
                else
                {
                    secBought = 0;
                }
            }
        }

        [KSPEvent(guiActive = true, guiName = "Deploy Laser Sail", active = true, guiActiveEditor = false)]
        public void DeployLaserSail()
        {
            print("[Sprite Mod] Laser Sail deployed");
            isDeployed = true;
            runAnimation(animName, deployAnimation, 1f, 0f);
        }

        [KSPEvent(guiActive = true, guiName = "Retract Laser Sail", active = false, guiActiveEditor = false)]
        public void RetractLaserSail()
        {
            print("[Sprite Mod] Laser Sail folded");
            isDeployed = false;
            isOn = false;
            runAnimation(animName, deployAnimation, -1f, 1f);
        }

        [KSPEvent(guiActive = true, guiName = "RUN LASERS", active = false)]
        public void RunLasers()
        {
            print("[Sprite Mod] Laser Sail reciving laser acceleration");
            isOn = true;
        }

        [KSPEvent(guiActive = true, guiName = "STOP LASERS", active = false)]
        public void StopLasers()
        {
            print("[Sprite Mod] Laser Sail stopped reciving laser acceleration");
            isOn = false;
        }

        [KSPEvent(guiActive = true, guiName = "Toggle Laser Sail", active = true, guiActiveEditor = true)]
        public void ToggleSail()
        {
            if (!isDeployed) DeployLaserSail();
            else RetractLaserSail();
        }

        [KSPAction("Toggle Sail")]
        public void toggleSailAction(KSPActionParam param)
        {
            if (!isDeployed) DeployLaserSail();
            else RetractLaserSail();
        }

        public override void OnStart(StartState state)
        {
            print("[Sprite Mod] New Part with LaserSailModule detected. laserForce = " + laserForce + " and animName: " + animName);
            deployAnimation = part.FindModelAnimators(animName).FirstOrDefault();
            if (isDeployed) {runAnimation(animName, deployAnimation, 1f, 0f); }

            //Action Group
            Actions["toggleSailAction"].active = true;

            //Editor Tabs
            if (state == StartState.Editor)
            {
                Fields["timeToBuy"].guiActive = false;
                Fields["timeToBuy"].guiActiveEditor = false;
                Events["ToggleSail"].active = true;
            }
            else
            {
                Fields["timeToBuy"].guiActive = true;
                Events["ToggleSail"].active = false;
            }
        }
 
        public override void OnUpdate()
        {
            Fields["status"].guiActive = true;
            Fields["secBoughtS"].guiActive = true;
            Fields["laserCostS"].guiActive = true;
            
            //Interface Events
            Events["buySailSeconds"].guiActive = true;
            Events["DeployLaserSail"].active = !isDeployed;
            Events["RetractLaserSail"].active = isDeployed;

            if (isDeployed)
            {
                Events["RunLasers"].active = !isOn;
                Events["StopLasers"].active = isOn;
            }
            else
            {
                Events["RunLasers"].active = false;
                Events["StopLasers"].active = false;
            }
            
            //Interface Text Data Output
            Fields["laserAccS"].guiActive = isDeployed;

            if (secBought > 0.001 || secBought == 0)secBoughtS = secBought.ToString("0.000") + " s";
            else secBoughtS = "0 s";
            laserAccS = (laserAccD/9.8066).ToString("0.000") + " g";
            laserCostS = laserCost.ToString("0.00") + " funds/s";
            if (!isDeployed)
            {
                status = "Idle";
            }
            else
            {
                if(!isOn)
                {
                    status = "Ready";
                }
                else
                {
                    double dT = TimeWarp.fixedDeltaTime;
                    if (DistanceToKerbin(this, vessel.orbit, this.part.transform.up, Planetarium.GetUniversalTime()) <= 70000)
                    {
                        status = "Too close to home planet";
                    }
                    else if(secBought < dT)
                    {
                        status = "Not enough seconds avilable";
                    }
                    else
                    {
                        status = "Accelerating";
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (!isOn) { return;}
            else
            {
                double dT = TimeWarp.fixedDeltaTime;

                if (secBought > dT && DistanceToKerbin(this, vessel.orbit, this.part.transform.up, Planetarium.GetUniversalTime()) > 70000)
                {
                    double UT = Planetarium.GetUniversalTime();
                    Vector3d finalForce = CalculateLaserForce(this, vessel.orbit, this.part.transform.up, UT);
                    Vector3d finalAcc = finalForce / vessel.GetTotalMass();

                    laserForceD = finalForce.magnitude;
                    laserAccD = finalAcc.magnitude;

                    vessel.ChangeWorldVelocity(finalAcc * dT);

                    secBought -= dT;
                }
            }
        }

        public static Vector3d CalculateLaserForce(LaserSailModule sail, Orbit orbit, Vector3d normal, double UT)
        {
             Vector3d homePosition = FlightGlobals.Bodies[1].getPositionAtUT(UT);
             Vector3d ownPosition = orbit.getPositionAtUT(UT);
             Vector3d relativePosition = ownPosition - homePosition;
             //if (Vector3d.Dot(normal, ownsunPosition) < 0)
             //{
             //    normal = -normal;
             //}

             double angleDeviation = Vector3.Dot(relativePosition.normalized, normal);


             Vector3d force = normal * (angleDeviation * sail.laserForce * Math.Pow(70000/(relativePosition.magnitude-600000),2) );
             return force;
        }

        public static double DistanceToKerbin(LaserSailModule sail, Orbit orbit, Vector3d normal, double UT)
        {
            Vector3d homePosition = FlightGlobals.Bodies[1].getPositionAtUT(UT);
            Vector3d ownPosition = orbit.getPositionAtUT(UT);
            Vector3d relativePosition = ownPosition - homePosition;

            double output = relativePosition.magnitude-600000;
            return output;
        }

        private void runAnimation(string animationMame, Animation anim, float speed, float aTime)
        {
            if (anim != null)
            {
                anim[animationMame].speed = speed;
                if (!anim.IsPlaying(animationMame))
                {
                    anim[animationMame].wrapMode = WrapMode.Default;
                    anim[animationMame].normalizedTime = aTime;
                    anim.Blend(animationMame, 1);
                }
            }
        }
    }
}
