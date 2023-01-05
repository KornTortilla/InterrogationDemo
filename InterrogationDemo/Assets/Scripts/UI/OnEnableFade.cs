//Variant of FadeInOut to fade in and out on enable and disable
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
