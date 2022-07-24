public enum SCENE_INDEX
{
    NONE = -1,
    Splash = 0,
    Main = 1,
    Level1 = 2,
    Level2 = 3,
    Lose = 4
}
public enum STATE
{
    IDLE_STATE,
    PLAY_STATE,
    LOSE_STATE,
    WIN_STATE,
    FLY_STATE,
    SHOP_STATE
}
public enum UI_PANEL
{
    NONE,
    MainScreen,
    ShopScreen,
    PlayScreen,
    LoseScreen,
    WinScreen,
    PopupListSong,
    PopupPause,
    PopupSetting,
    PopupMisson,
    PopupContinue,
    PopupWarning,
    PopupNoInternet,
    PopupAdGold
}
public enum PANEL_TYPE
{
    NONE,
    UI,
    SCREEN,
    POPUP,
    NOTIFICATION,
    LOADING,
}
public enum TYPE_SOUND
{
    NONE = -1,
    CONFIRM = 0,
    TOUCH = 1,
    GAME_OVER = 2,
    WIN = 3,
    RED_PLAY = 4,
    RED_PRESS = 5,
    BLUE_PLAY = 6,
    BLUE_PRESS = 7,
    GREEN_PLAY = 8,
    GREEN_PRESS = 9,
    YELLOW_PLAY = 10,
    YELLOW_PRESS = 11
}
