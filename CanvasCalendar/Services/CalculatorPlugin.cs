using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace CanvasCalendar.Services;

/// <summary>
/// Simple calculator plugin for testing Semantic Kernel function calling.
/// </summary>
public class CalculatorPlugin
{
    /// <summary>
    /// Adds two numbers together.
    /// </summary>
    /// <param name="number1">The first number</param>
    /// <param name="number2">The second number</param>
    /// <returns>The sum of the two numbers</returns>
    [KernelFunction("add_numbers")]
    [Description("Adds two numbers together and returns the result")]
    public int AddNumbers(
        [Description("The first number to add")] int number1,
        [Description("The second number to add")] int number2)
    {
        // Console output to verify the function was actually called
        Console.WriteLine("🔥 CALCULATOR FUNCTION CALLED! 🔥");
        Console.WriteLine($"📊 Adding {number1} + {number2}");
        
        var result = number1 + number2;
        
        Console.WriteLine($"✅ Result: {result}");
        Console.WriteLine("🎯 Function execution completed!");
        
        return result;
    }

    /// <summary>
    /// Multiplies two numbers together.
    /// </summary>
    /// <param name="number1">The first number</param>
    /// <param name="number2">The second number</param>
    /// <returns>The product of the two numbers</returns>
    [KernelFunction("multiply_numbers")]
    [Description("Multiplies two numbers together and returns the result")]
    public int MultiplyNumbers(
        [Description("The first number to multiply")] int number1,
        [Description("The second number to multiply")] int number2)
    {
        // Console output to verify the function was actually called
        Console.WriteLine("🚀 MULTIPLY FUNCTION CALLED! 🚀");
        Console.WriteLine($"🔢 Multiplying {number1} × {number2}");
        
        var result = number1 * number2;
        
        Console.WriteLine($"✅ Result: {result}");
        Console.WriteLine("🎯 Multiply function execution completed!");
        
        return result;
    }
}
