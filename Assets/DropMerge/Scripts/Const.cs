namespace DropMerge
{
    public static class Tags
    {
        public static readonly string Cat = "Cat";
    }

    public static class Scenes
    {
        public static readonly string Game = "Game";
    }

    public static class GameStates
    {
        public static readonly string Init = "init";
        public static readonly string Playing = "playing";
        public static readonly string Lose = "lose";
        public static readonly string End = "end";

        public static readonly string Action_Collider = "action_collider";
        public static readonly string Action_Retry = "action_retry";
        public static readonly string Action_Continue = "action_continue";
        public static readonly string Action_EndGame = "action_end_game";
        public static readonly string Action_SaveGame = "action_save_game";
    }
}
