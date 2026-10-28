namespace SchoolBot.Installer;

internal sealed class MsiProgress
{
    private long total;
    private long position;
    private int direction = 1;
    private int actionStep;
    internal bool IsEstimating { get; private set; } = true;
    internal double Percent => total > 0 ? Math.Clamp(100.0 * position / total, 0, 100) : 0;

    internal void Update(int kind, int amount, int flag, int phase)
    {
        switch (kind)
        {
            case 0:
                total = Math.Max(0, amount);
                direction = flag == 0 ? 1 : -1;
                position = direction == 1 ? 0 : total;
                IsEstimating = phase == 1;
                actionStep = 0;
                break;
            case 1:
                actionStep = flag != 0 ? Math.Max(0, amount) : 0;
                break;
            case 2:
                position += direction * (long)amount;
                break;
            case 3:
                total = Math.Max(0, total + amount);
                break;
        }
    }

    internal void StartAction() => actionStep = 0;
    internal void ActionData() => position += direction * (long)actionStep;
}
