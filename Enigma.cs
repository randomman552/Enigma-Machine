using System;
using Windows.UI.WebUI;

//TODO: Switchboard implementation

/// <summary>
/// Class representing and enigma machine, can encode and decode text.
/// </summary>
public class Enigma
{
	/// <summary>
	/// Wheel states
	/// Can be set using the public Wheels property.
	/// </summary>
	private int[] ws;


	/// <summary>
	/// Wheel states (NOTE: this is a copy of the wheel states and cannot be directly modified).
	/// </summary>
	public int[] Wheels
    {
		get{ return (int[])ws.Clone(); }
		set{
			if (!(value is null) && value.Length == 3) {
				ws = (int[])value.Clone();
			} 
			else
            {
				throw new ArgumentException("Wheel states cannot be null and must have a length of 3.");
            }
		}
    }


	/// <param name="wheels">The inital wheel values (list of length 3). If unspecified random numbers will be used.</param>
	public Enigma(int[] wheels = null)
	{
		if (wheels is null)
        {
			RandomiseWheels();
        }
        else
        {
			Wheels = wheels;
        }
	}


	/// <summary>
	/// Randomise the wheel values
	/// Does NOT use a cryptographically secure source of randomness.
	/// </summary>
	public void RandomiseWheels()
    {
		Random rnd = new Random();
		Wheels = new int[] { rnd.Next(26), rnd.Next(26), rnd.Next(26) };
    }


	/// <summary>
	/// Encode the given string
	/// </summary>
	/// <param name="text">The string to encode</param>
	/// <returns>The encoded ciphertext</returns>
	public String Encode(String text)
    {
		String result = "";

		for (int i = 0; i < text.Length; i++)
		{
			result += EncodeChar(text[i]);
		}

		return result;
	}


	/// <summary>
	/// Decode the given string
	/// </summary>
	/// <param name="text">The string to decode</param>
	/// <returns>The decoded plaintext</returns>
	public String Decode(String text)
    {
		String result = "";

        for (int i = 0; i < text.Length; i++)
        {
			result += DecodeChar(text[i]);
        }

		return result;
    }


	/// <summary>
	/// Correct implementation of modulo operator, as opposed to C#'s remainder operator (%)
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	private int Mod(int a, int b)
    {
		return (a % b + b) % b;
    }


	/// <summary>
	/// Encode the given character
	/// </summary>
	/// <param name="c">The character to encode</param>
	/// <returns>The newly encoded character</returns>
	public char EncodeChar(char c)
	{
		//Convert to lower case, and subtract 97 to make 'a' 0.
		c = (char)(Char.ToLower(c) - 97);

		//Apply the wheels to the character, then add 97 to make 0 into 'a' again.
		c = (char)(Mod(c + ws[0] + ws[1] + ws[2], 25) + 97);

		AdvanceWheels();
		return c;
	}


	/// <summary>
	/// Decode the given character
	/// </summary>
	/// <param name="c">The character to decode</param>
	/// <returns>The newly decoded character</returns>
	public char DecodeChar(char c)
    {
		//Convert to lower case, and subtract 97 to make 'a' 0.
		c = (char)(Char.ToLower(c) - 97);

		//Apply the wheels to the character, then add 97 to make 0 into 'a' again.
		c = (char)(Mod(c - (ws[0] + ws[1] + ws[2]), 25) + 97);

		AdvanceWheels();
		return c;
	}


	/// <summary>
	/// Advance the wheels by one state, this will handle rollover to the next wheel.
	/// </summary>
	private void AdvanceWheels()
    {
		//Probably a better way to do this rather than nested if statements, but this works for now
		ws[0]++;

        for (int i = 0; i < ws.Length; i++)
        {
			if (ws[i] > 25)
            {
				//Advance next wheel if there is one
				if (i + 1 < ws.Length)
				{
					ws[i + 1]++;
				}
				
				//Wrap current wheel around
				ws[i] %= 25;
            }
        }
    }
}
