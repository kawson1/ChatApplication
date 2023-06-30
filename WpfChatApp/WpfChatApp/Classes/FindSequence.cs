using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChatApp.Classes
{
    internal class FindSequence
    {
        public static int GetIndexOfSequenceStart<T>(T[] array, T[] sequence)
        {
            int arrayLength = array.Length;
            int sequenceLength = sequence.Length;

            for (int i = 0; i <= arrayLength - sequenceLength; i++)
            {
                bool found = true;
                for (int j = 0; j < sequenceLength; j++)
                {
                    if(!EqualityComparer<T>.Default.Equals(array[i + j], sequence[j]))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i;
                }
            }

            return -1; // Indeks -1 oznacza brak znalezienia sekwencji
        }

    }
}
