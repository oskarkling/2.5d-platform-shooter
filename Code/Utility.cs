using System.Collections;
using System.Collections.Generic;

public static class Utility
{
    //Tar in en array och gör en random i den.
    public static T[] ShuffleArray<T>(T[] array, int seed)
     {
         System.Random randomGenerator = new System.Random(seed);

         for(int i = 0; i < array.Length-1; i++)
         {
             int randomIndex = randomGenerator.Next(i, array.Length);
             T tempItem = array[randomIndex];
             array[randomIndex] = array[i];
             array[i] = tempItem;
         }
         return array;
     }
}
