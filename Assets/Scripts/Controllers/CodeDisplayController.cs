using System;

public class CodeDisplayController : IDisposable
{
    public event Action<string> CodeGenerated;

    private readonly ICodeDisplayView view;
    private readonly ICodeGenerator codeGenerator;

    private readonly int codeLength;
    private readonly float timerDurationSeconds;

    private float remainingTime;
    private bool timerRunning;

    public CodeDisplayController(
        ICodeDisplayView view,
        ICodeGenerator codeGenerator,
        int codeLength = 6,
        float timerDurationSeconds = 60f) 
    {
        this.view = view ?? throw new ArgumentNullException(nameof(view));
        this.codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        this.codeLength = codeLength;
        this.timerDurationSeconds = timerDurationSeconds;

        this.view.GenerateButtonClicked += OnGenerateButtonClicked;

        this.view.SetCodeText("------");
        this.view.SetTimerText("60");
    }

    private void OnGenerateButtonClicked()
    {
        string generatedCode = codeGenerator.Generate(codeLength);

        view.SetCodeText(generatedCode);

        remainingTime = timerDurationSeconds;
        timerRunning = true;
        view.SetTimerText(Math.Ceiling(remainingTime).ToString("00"));

        CodeGenerated?.Invoke(generatedCode);
    }

    public void Tick(float deltaTime)
    {
        if (!timerRunning)
        {
            return;
        }

        remainingTime -= deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;
        }

        view.SetTimerText(Math.Ceiling(remainingTime).ToString("00"));
    }

    public void Dispose()
    {
        view.GenerateButtonClicked -= OnGenerateButtonClicked;
    }
}