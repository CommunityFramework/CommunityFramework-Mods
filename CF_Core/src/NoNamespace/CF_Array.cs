using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CF_Array
{
    public static bool ContainsNonNull<T>(T[] items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                return true;
            }
        }
        return false;
    }
}
