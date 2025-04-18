using Rpn;

namespace RpnComponents
{
    public static class EnemyHpCalculator
    {
        public static float CalculateHp(string rpnExpression, int wave, float baseHp = 0)
        {
            // Replace "wave" and "base" with dynamic values from JSON
            string expression = rpnExpression.Replace("wave", wave.ToString())
                                            .Replace("base", baseHp.ToString());
            return RpnEvaluator.JSONloader(expression);
        }
    }
}