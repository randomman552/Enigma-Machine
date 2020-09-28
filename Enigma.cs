using System;
using System.Collections.Generic;


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
				if (!(OnWheelChange is null))
				{
					OnWheelChange(Wheels);
				}
			} 
			else
            {
				throw new ArgumentException("Wheel states cannot be null and must have a length of 3.");
            }
		}
    }


	/// <summary>
	/// Character mapping for the switch board.
	/// MUST be interacted with using the LinkChars and UnlinkChar methods.
	/// When a link is made, it must be made in both directions (link 'a' to 'b' and 'b' to 'a' in this dict).
	/// </summary>
	private Dictionary<char, char> sb = new Dictionary<char, char>();


	/// <summary>
	/// The switch board dictionary mappings.
	/// NOTE: This dictionary cannot be altered to change the internal switch board dict.
	/// </summary>
	public Dictionary<char, char> SwitchBoard {
		get
        {
			return new Dictionary<char, char>(sb);
        }
		set
        {
			//We do this instead of just introducing the new value so that any link/unlink events are still called properly
			//Unlink all old pairs
            foreach (var pair in SwitchBoard)
            {
				UnlinkChar(pair.Key);
            }

			//Link all new pairs
            foreach (var pair in value)
            {
				LinkChars(pair.Key, pair.Value);
            }
        }
	}


	/// <summary>
	/// Random source used for randomising encoding values
	/// </summary>
	private Random rnd = new Random();


	/// <summary>
	/// Method to be called whenever the wheels are changed.
	/// Should take an integer array as an argument, this represents the wheel states.
	/// </summary>
	public Action<int[]> OnWheelChange;


	/// <summary>
	/// Method to be called whenever a switch board link is made.
	/// Should take two chars as arguments, the chars are the two characters which were just linked.
	/// </summary>
	public Action<char, char> OnSwitchBoardLink;


	/// <summary>
	/// Method to be called whenever a switch board link is removed.
	/// Should take two characters as arguments, (the characters which were just unlinked.
	/// </summary>
	public Action<char, char> OnSwitchBoardUnlink;


	/// <param name="wheels">The inital wheel values (list of length 3). If unspecified random numbers will be used.</param>
	public Enigma(int[] wheels = null)
	{
		if (wheels is null)
        {
			Wheels = new int[] { 0, 0, 0 };
        }
        else
        {
			Wheels = wheels;
        }
	}


	/// <summary>
	/// Randomise the wheel values.
	/// Does NOT use a cryptographically secure source of randomness.
	/// </summary>
	public void RandomiseWheels()
    {
		Wheels = new int[] { rnd.Next(26), rnd.Next(26), rnd.Next(26) };
    }


	/// <summary>
	/// Randomise the switch board values.
	/// Does NOT use a cryptographically secure source of randomness.
	/// </summary>
	public void RandomiseSwitchBoard()
	{
		//Unlink all old pairs
		foreach (var pair in SwitchBoard)
		{
			UnlinkChar(pair.Key);
		}

		//For each character, pair with another random character
		for (int i = 97; i < 123; i++)
        {
			while (!sb.ContainsKey((char)i))
            {
				char toLink = (char)rnd.Next(97, 123);

				//Link the characters if toLink has not yet been linked to another character
				if (!sb.ContainsKey(toLink))
                {
					LinkChars((char)i, toLink);
				}
            }
        }
    }


	/// <summary>
	/// Randomise the internal state of the enigma object
	/// </summary>
	public void Randomise()
    {
		RandomiseWheels();
		RandomiseSwitchBoard();
    }


	/// <summary>
	/// Encode the given string
	/// </summary>
	/// <param name="text">The string to encode</param>
	/// <returns>The encoded ciphertext</returns>
	public string Encode(string text)
    {
		string result = "";

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
	public string Decode(string text)
    {
		string result = "";

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
		//Convert to lower case and apply switch board
		c = Char.ToLower(c);
		c = ApplySwitchBoard(c);

		if (Char.IsLetter(c))
		{
			//Subtract 'a' to make 'a' equal to 0
			c -= 'a';

			//Apply the wheels to the character, then add 97 to make 0 into 'a' again.
			c = (char)(Mod(c + ws[0] + ws[1] + ws[2], 25) + 97);

			AdvanceWheels();
		}
		return c;
	}


	/// <summary>
	/// Decode the given character
	/// </summary>
	/// <param name="c">The character to decode</param>
	/// <returns>The newly decoded character</returns>
	public char DecodeChar(char c)
    {
		//Convert to lower case
		c = Char.ToLower(c);

		if (Char.IsLetter(c))
		{
			//Subtract 'a' to make 'a' equal to 0
			c -= 'a';

			//Apply the wheels to the character, then add 97 to make 0 into 'a' again.
			c = (char)(Mod(c - (ws[0] + ws[1] + ws[2]), 25) + 97);

			AdvanceWheels();
		}
		//Apply switch board after processing to return character to original state
		c = ApplySwitchBoard(c);

		return c;
	}


	/// <summary>
	/// Advance the wheels 1 state, this will handle rollover to the next wheel.
	/// </summary>
	public void AdvanceWheels()
    {
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

		//Run the on wheel advance method if it is set
		if (!(OnWheelChange is null))
		{
			OnWheelChange(Wheels);
		}
	}


	/// <summary>
	/// Link the given characters together on the switch board
	/// </summary>
	/// <param name="a">First character</param>
	/// <param name="b">Second character</param>
	public void LinkChars(char a, char b)
    {
		//Convert both characters to lower case
		a = Char.ToLower(a);
		b = Char.ToLower(b);

		//Continue only if a link has not already been made, and the characters are not the same
		if (!(sb.ContainsKey(a) || sb.ContainsKey(b)))
		{
			if (a != b)
			{
				sb[a] = b;
				sb[b] = a;

				//Call the on link action if it is set
				if (!(OnSwitchBoardLink is null))
				{
					OnSwitchBoardLink(a, b);
				}
			}
		}
        else
        {
			//If characters are already linked, unlink them and call this function again
			UnlinkChar(a);

			//Switch the argument order for the recursive step, so that b is also unlinked if required
			LinkChars(b, a);
        }
    }


	/// <summary>
	/// Remove any references of the given chracter from the switch board
	/// </summary>
	/// <param name="a">The character the unlink</param>
	public void UnlinkChar(char a)
    {
		if (sb.ContainsKey(a))
		{
			char b = sb[a];
			sb.Remove(a);
			sb.Remove(b);

			//Call the unlink action if it is set
			if (!(OnSwitchBoardUnlink is null))
			{
				OnSwitchBoardUnlink(a, b);
			}
		}
    }


	/// <summary>
	/// Apply the switch board to the given character
	/// </summary>
	/// <param name="c">The character to apply to switch board to</param>
	/// <returns>The character transformed by the switch board</returns>
	private char ApplySwitchBoard(char c)
    {
		if (sb.ContainsKey(c))
		{
			c = sb[c];
		}
		return c;
	}
}
