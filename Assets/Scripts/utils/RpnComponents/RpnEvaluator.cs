using System.Collections.Generic;
using UnityEngine;

namespace Rpn
{
    public class RpnEvaluator
    {
        public static float JSONloader(string expression)
        {
            Stack<float> nums = new Stack<float>();
            string[] tokens = expression.Split(' ');

            foreach (string token in tokens)
            {
                if (float.TryParse(token, out float number))
                {
                    // If the token is number, push
                    nums.Push(number);
                }
                else
                {
                    if (nums.Count < 2)
                    {
                        Debug.LogError("Invalid RPN");
                        return float.NaN; // error
                    }

                    float num2 = nums.Pop();
                    float num1 = nums.Pop();
                    float result = 0;

                    switch (token)
                    {
                        case "+":
                            result = num1 + num2;
                            break;
                        case "-":
                            result = num1 - num2;
                            break;
                        case "*":
                            result = num1 * num2;
                            break;
                        case "/":
                            if (num2 == 0)
                            {
                                Debug.LogError("Division by zero in RPN expression.");
                                return float.NaN; // error
                            }
                            result = num1 / num2;
                            break;
                        default:
                            Debug.LogError($"Invalid operator: {token}");
                            return float.NaN; // Return NaN to indicate an error
                    }

                    nums.Push(result);
                }
            }

            if (nums.Count != 1)
            {
                Debug.LogError("Invalid RPN expression: too many operands.");
                return float.NaN; // error
            }

            return nums.Pop(); // final value
        }
    }
}