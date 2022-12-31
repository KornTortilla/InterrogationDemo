public class OnEnableFade : FadeInOut
{
    void OnEnable()
    {
        FadeIn();
    }

    void OnDisable()
    {
        FadeOut();
    }
}
