using UnityEngine;

public class SkyProcessor : MonoBehaviour, IService
{
    [SerializeField] private Material skyMaterial;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    public void LerpSky(Material start, Material end, DateTimeType type, float curDateTime)
    {
        float lerp = 0;
        switch (type)
        {
            case DateTimeType.AM:
                lerp = curDateTime / 100;
                break;
            case DateTimeType.PM:
                lerp = (curDateTime - 100) / 200;
                break;
        }

        SetValue(
            Color.Lerp(start.GetColor("_SunDiscColor"), end.GetColor("_SunDiscColor"), lerp),
            Mathf.Lerp(start.GetFloat("_SunDiscMultiplier"), end.GetFloat("_SunDiscMultiplier"), lerp),
            Mathf.Lerp(start.GetFloat("_SunDiscExponent"), end.GetFloat("_SunDiscExponent"), lerp),

            Color.Lerp(start.GetColor("_SunHaloColor"), end.GetColor("_SunHaloColor"), lerp),
            Mathf.Lerp(start.GetFloat("_SunHaloExponent"), end.GetFloat("_SunHaloExponent"), lerp),
            Mathf.Lerp(start.GetFloat("_SunHaloContribution"), end.GetFloat("_SunHaloContribution"), lerp),

            Color.Lerp(start.GetColor("_HorizonLineColor"), end.GetColor("_HorizonLineColor"), lerp),
            Mathf.Lerp(start.GetFloat("_HorizonLineExponent"), end.GetFloat("_HorizonLineExponent"), lerp),
            Mathf.Lerp(start.GetFloat("_HorizonLineContribution"), end.GetFloat("_HorizonLineContribution"), lerp),

            Color.Lerp(start.GetColor("_SkyGradientTop"), end.GetColor("_SkyGradientTop"), lerp),
            Color.Lerp(start.GetColor("_SkyGradientBottom"), end.GetColor("_SkyGradientBottom"), lerp),
            Mathf.Lerp(start.GetFloat("_SkyGradientExponent"), end.GetFloat("_SkyGradientExponent"), lerp)
        );
    }

    private void SetValue(
      Color _SunDiscColor,
      float _SunDiscMultiplier,
      float _SunDiscExponent,

      Color _SunHaloColor,
      float _SunHaloExponent,
      float _SunHaloContribution,

      Color _HorizonLineColor,
      float _HorizonLineExponent,
      float _HorizonLineContribution,

      Color _SkyGradientTop,
      Color _SkyGradientBottom,
      float _SkyGradientExponent
    )
    {
        skyMaterial.SetColor("_SunDiscColor", _SunDiscColor);
        skyMaterial.SetFloat("_SunDiscMultiplier", _SunDiscMultiplier);
        skyMaterial.SetFloat("_SunDiscExponent", _SunDiscExponent);

        skyMaterial.SetColor("_SunHaloColor", _SunHaloColor);
        skyMaterial.SetFloat("_SunHaloExponent", _SunHaloExponent);
        skyMaterial.SetFloat("_SunHaloContribution", _SunHaloContribution);

        skyMaterial.SetColor("_HorizonLineColor", _HorizonLineColor);
        skyMaterial.SetFloat("_HorizonLineExponent", _HorizonLineExponent);
        skyMaterial.SetFloat("_HorizonLineContribution", _HorizonLineContribution);

        skyMaterial.SetColor("_SkyGradientTop", _SkyGradientTop);
        skyMaterial.SetColor("_SkyGradientBottom", _SkyGradientBottom);
        skyMaterial.SetFloat("_SkyGradientExponent", _SkyGradientExponent);
    }
}