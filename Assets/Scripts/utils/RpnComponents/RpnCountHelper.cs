using Rpn;
using UnityEngine;

namespace RpnComponents
{
    public static class EnemyCountCalculator
    {
        public static int CalculateCount(string rpnExpression, int wave)
        {
            // Replace "wave" with dynamic values from JSON
            string expression = rpnExpression.Replace("wave", wave.ToString());
            float result = RpnEvaluator.JSONloader(expression);
            return Mathf.RoundToInt(result);
        }
    }
}