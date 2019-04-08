using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Roleplay.Misc
{
    public class JailBreak
    {
        #region Variables
        GameClient UserJailbreaking;
        FlexTimer JailbreakTimer;
        FlexTimer CoordCheckTimer;

        RoomItem Banzai;
        int BanzaiTileX;
        int BanzaiTileY;
        uint BanzaiTileRoomID;

        long JailbreakTime = 300000;
        long CoordCheckTime = 2000;
        #endregion

        #region Constructor
        public JailBreak(GameClient UserJailbreaking, int BanzaiTileX, int BanzaiTileY, uint BanzaiTileRoomID, RoomItem Banzai)
        {
            this.UserJailbreaking = UserJailbreaking;
            this.BanzaiTileX = BanzaiTileX;
            this.BanzaiTileY = BanzaiTileY;
            this.BanzaiTileRoomID = BanzaiTileRoomID;
            this.Banzai = Banzai;

            ChangeItemState("1");
            CreateAndStartTimers();
        }
        #endregion

        #region Timers
        public void CreateAndStartTimers()
        {
            JailbreakTimer = new FlexTimer(JailbreakTimerFinished, JailbreakTimerTicked, null, JailbreakTime, 60000);
            CoordCheckTimer = new FlexTimer(CoordTimerFinished, CoordTimerTicked, null, 0, CoordCheckTime, true);
        }

        public void JailbreakTimerFinished(object NullObject)
        {
            if (IsAnythingNull() || !UserInPosition())
            { StopJailbreak(); return; }

            RoleplayManager.Shout(UserJailbreaking, "*Cuts the last wire from the prison fence, freeing all the convicts inside*");

            FreeAllConvicts();
            StopJailbreak(true);
        }

        public void CoordTimerFinished(object NullObject)
        {
        }

        public void JailbreakTimerTicked(object NullObject)
        {
            if (IsAnythingNull() || !UserInPosition())
            { StopJailbreak(); return; }

            RoleplayManager.Shout(UserJailbreaking, "*Carries on cutting the wires from the prison fence [" + GetJailbreakTimeLeft() + " minutes left]*");
        }

        public void CoordTimerTicked(object NullObject)
        {
            if (IsAnythingNull() || !UserInPosition())
            { StopJailbreak(); return; }

        }
        #endregion

        #region Private Methods
        bool IsAnythingNull()
        {
            if (UserJailbreaking == null || UserJailbreaking.GetHabbo() == null || UserJailbreaking.GetHabbo().Disconnected ||
                UserJailbreaking.GetHabbo().GetRoomUser() == null || UserJailbreaking.GetRoleplay() == null ||
                UserJailbreaking.GetHabbo().CurrentRoom == null || Banzai == null)
            {
                return true;
            }
            else
                return false;
        }

        void ChangeItemState(string State)
        {
            Banzai.ExtraData = State;
            Banzai.UpdateState(false, true);
        }

        long GetJailbreakTimeLeft()
        {
            return JailbreakTimer.TimeRemaining / 60000;
        }

        bool UserInPosition()
        {
            if (IsAnythingNull())
            { StopJailbreak(); return false; }

            Vector2D UserPosition = new Vector2D(UserJailbreaking.GetHabbo().GetRoomUser().X, UserJailbreaking.GetHabbo().GetRoomUser().Y);
            Vector2D BanzaiTilePosition = new Vector2D(BanzaiTileX, BanzaiTileY);

            if (UserPosition.X != BanzaiTilePosition.X || UserPosition.Y != BanzaiTilePosition.Y)
                return false;
            if (UserJailbreaking.GetHabbo().CurrentRoom.RoomId != BanzaiTileRoomID)
                return false;
            return true;
        }

        void FreeAllConvicts()
        {
            lock (Plus.GetGame().GetClientManager().Clients.Values)
            {
                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {
                    if (client == null || client.GetHabbo() == null || client.GetRoleplay() == null || !client.GetRoleplay().Jailed)
                        continue;
                    UpdateUserLook(client);

                    client.GetRoleplay().Jailed = false;
                    client.GetRoleplay().JailTimer = 0;
                    client.GetRoleplay().SaveStatusComponents("jailed");
                    client.SendWhisper("Você escapou da prisão!");

                    HabboHotel.Rooms.RoomUser RUser = client.GetHabbo().GetRoomUser();
                    if (RUser == null)
                        continue;
                    RUser.SetPos(23, 19, 0.2);
                    RUser.ClearMovement();
                }
            }
        }

        void UpdateUserLook(GameClient Session)
        {
            if (Session.GetRoleplay().MottBeforeSpecial != null)
            {
                Session.GetHabbo().Look = Session.GetRoleplay().FigBeforeSpecial;
                Session.GetHabbo().Motto = Session.GetRoleplay().MottBeforeSpecial;
            }

            Session.GetRoleplay().RefreshVals();
        }
        #endregion

        #region Public Methods
        public void StopJailbreak(bool Success = false)
        {
            CoordCheckTimer.StopFlexTimer();
            JailbreakTimer.StopFlexTimer();
            ChangeItemState("0");

            Plus.GetGame().JailBreak = null;

            if (IsAnythingNull() || Success)
                return;
            RoleplayManager.Shout(UserJailbreaking, "*Stops cutting the wires on the fence*");
        }
        #endregion
    }
}
