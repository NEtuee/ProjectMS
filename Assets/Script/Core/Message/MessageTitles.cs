

public static partial class MessageTitles
{

    public const ushort     customTitle_start           = 0xff00;

    public const ushort     system_registerRequest      = 0x0100;
    public const ushort     system_deregisterRequest    = 0x0101;
    public const ushort     system_onTriggerEnter       = 0x0102;
    public const ushort     system_onTriggerExit        = 0x0103;

    //게임 시작 알림
    public const ushort     game_gameStart            = 0x0200;
    //게임 오버 알림
    public const ushort     game_gameOver        = 0x0201;
    //맵 변경 알림
    public const ushort     game_mapChange       = 0x0202;
    //타겟 텔레포트
    public const ushort     game_teleportTarget         = 0x0203;


}
