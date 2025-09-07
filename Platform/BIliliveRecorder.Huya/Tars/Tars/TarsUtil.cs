namespace BIliliveRecorder.Huya.Tars.Tars;

internal class TarsUtil
{
    /**
         * Constant to use in building the hashCode.
         */
    private static int iConstant = 37;

    /**
     * Running total of the hashCode.
     */
    private static int iTotal = 17;

    public static bool Equals(bool l, bool r)
    {
        return l == r;
    }

    public static bool Equals(byte l, byte r)
    {
        return l == r;
    }

    public static bool Equals(char l, char r)
    {
        return l == r;
    }

    public static bool Equals(short l, short r)
    {
        return l == r;
    }

    public static bool Equals(int l, int r)
    {
        return l == r;
    }

    public static bool Equals(long l, long r)
    {
        return l == r;
    }

    public static bool Equals(float l, float r)
    {
        return l == r;
    }

    public static bool Equals(double l, double r)
    {
        return l == r;
    }

    public new static bool Equals(object l, object r)
    {
        return l.Equals(r);
    }

    public static int compareTo(bool l, bool r)
    {
        return (l ? 1 : 0) - (r ? 1 : 0);
    }

    public static int compareTo(byte l, byte r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo(char l, char r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo(short l, short r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo(int l, int r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo(long l, long r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo(float l, float r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo(double l, double r)
    {
        return l < r ? -1 : l > r ? 1 : 0;
    }

    public static int compareTo<T>(T l, T r) where T : IComparable
    {
        return l.CompareTo(r);
    }

    public static int compareTo<T>(List<T> l, List<T> r) where T : IComparable
    {
        var n = 0;
        for (int i = 0, j = 0; i < l.Count && j < r.Count; i++, j++)
        {
            if (l[i] is IComparable && r[j] is IComparable)
            {
                var lc = (IComparable)l[i];
                var rc = (IComparable)r[j];
                n = lc.CompareTo(rc);
                if (n != 0)
                {
                    return n;
                }
            }
            else
            {
                throw new Exception("Argument must be IComparable!");
            }
        }

        return compareTo(l.Count, r.Count);
    }

    public static int compareTo<T>(T[] l, T[] r) where T : IComparable
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = l[li].CompareTo(r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(bool[] l, bool[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(byte[] l, byte[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(char[] l, char[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(short[] l, short[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(int[] l, int[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(long[] l, long[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(float[] l, float[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int compareTo(double[] l, double[] r)
    {
        for (int li = 0, ri = 0; li < l.Length && ri < r.Length; ++li, ++ri)
        {
            var n = compareTo(l[li], r[ri]);
            if (n != 0)
                return n;
        }

        return compareTo(l.Length, r.Length);
    }

    public static int hashCode(bool o)
    {
        return iTotal * iConstant + (o ? 0 : 1);
    }

    public static int hashCode(bool[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + (t ? 0 : 1));
        }
    }

    public static int hashCode(byte o)
    {
        return iTotal * iConstant + o;
    }

    public static int hashCode(byte[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + t);
        }
    }

    public static int hashCode(char o)
    {
        return iTotal * iConstant + o;
    }

    public static int hashCode(char[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + t);
        }
    }

    public static int hashCode(double o)
    {
        return hashCode(Convert.ToInt64(o));
    }

    public static int hashCode(double[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + (int)(Convert.ToInt64(t) ^ (Convert.ToInt64(t) >> 32)));
        }
    }

    public static int hashCode(float o)
    {
        return iTotal * iConstant + Convert.ToInt32(o);
    }

    public static int hashCode(float[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + Convert.ToInt32(t));
        }
    }

    public static int hashCode(short o)
    {
        return iTotal * iConstant + o;
    }

    public static int hashCode(short[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + t);
        }
    }


    public static int hashCode(int o)
    {
        return iTotal * iConstant + o;
    }

    public static int hashCode(int[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + t);
        }
    }

    public static int hashCode(long o)
    {
        return iTotal * iConstant + (int)(o ^ (o >> 32));
    }

    public static int hashCode(long[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + (int)(t ^ (t >> 32)));
        }
    }

    public static int hashCode(TarsStruct[] array)
    {
        if (array == null)
        {
            return iTotal * iConstant;
        }
        else
        {
            return array.Aggregate(iTotal, (current, t) => current * iConstant + t.GetHashCode());
        }
    }


    public static int hashCode(object obj)
    {
        if (null == obj)
        {
            return iTotal * iConstant;
        }
        else
        {
            if (obj.GetType().IsArray)
            {
                if (obj is long[])
                {
                    return hashCode((long[])obj);
                }
                else if (obj is int[])
                {
                    return hashCode((int[])obj);
                }
                else if (obj is short[])
                {
                    return hashCode((short[])obj);
                }
                else if (obj is char[])
                {
                    return hashCode((char[])obj);
                }
                else if (obj is byte[])
                {
                    return hashCode((byte[])obj);
                }
                else if (obj is double[])
                {
                    return hashCode((double[])obj);
                }
                else if (obj is float[])
                {
                    return hashCode((float[])obj);
                }
                else if (obj is bool[])
                {
                    return hashCode((bool[])obj);
                }
                else if (obj is TarsStruct[])
                {
                    return hashCode((TarsStruct[])obj);
                }
                else
                {
                    return hashCode((Object[])obj);
                }
            }
            else if (obj is TarsStruct)
            {
                return obj.GetHashCode();
            }
            else
            {
                return iTotal * iConstant + obj.GetHashCode();
            }
        }
    }

    public static byte[] GetTarsBufArray(MemoryStream ms)
    {
        var bytes = new byte[ms.Position];
        Array.Copy(ms.GetBuffer(), 0, bytes, 0, bytes.Length);
        return bytes;
    }
}
