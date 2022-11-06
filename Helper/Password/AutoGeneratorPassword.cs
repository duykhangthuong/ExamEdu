using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.Helper
{
    public class AutoGeneratorPassword
    { 
        /// <summary>
        /// Create a random password 
        /// </summary>
        /// <param name="lenght">the lenght of password</param>
        /// <param name="numberOfLowerCase">the minimum number of lower case on the password</param>
        /// <param name="numberOfUpperCase">the minimum number of upper case on the password</param>
        /// <param name="number">the minimum number of number on the password</param>
        /// <returns>a single random password</returns>
        public static string passwordGenerator(int lenght, int numberOfLowerCase, int numberOfUpperCase, int number){
            
            string password = "";
		    int charLeft = lenght - numberOfLowerCase - numberOfUpperCase - number;
            if(charLeft < 0){
                throw new Exception("Number Of Lowercase or Uppercase or number can't be larger than length of password");
            }
            string upperList = "QWERTYUIOPASDFGHJKLZXCVBNM";
            string lowerList = "qwertyuiopasdfghjklzxcvbnm";
            string numberList = "0123456789";
            Random random = new Random();
            string upperListRandom ="";
            string lowerListRandom ="";
            string numberListRandom ="";
            string charLeftList ="";
            for(int i = 0; i<numberOfUpperCase;i++){
                upperListRandom += upperList[random.Next(0,upperList.Length-1)];
            }
            for(int i = 0; i<numberOfLowerCase;i++){
                lowerListRandom += lowerList[random.Next(0,lowerList.Length-1)];
            }
            for(int i = 0; i<number;i++){
                numberListRandom += numberList[random.Next(0,numberList.Length-1)];
            }
            for(int i = 0; i<charLeft;i++){
                string totalList = upperList + lowerList + numberList;
                charLeftList += totalList[random.Next(0,totalList.Length-1)];
            }
            password = upperListRandom + lowerListRandom + numberListRandom + charLeftList;
            password = Shuffle(password);
            return password;
	}
    /// <summary>
        /// Shuffle a string 
        /// </summary>
        /// <param name="str">String to shuffle</param>
        /// <returns>a shuffled string</returns>
    private static string Shuffle(string str)
    {
        char[] array = str.ToCharArray();
        Random rng = new Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }
    }

}