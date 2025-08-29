using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Utility
{
    public static float EulerRotationValueNormalized(float value)
    {
        float yNormalized = value % 360f;
        if (yNormalized < 0) yNormalized += 360f;
        return yNormalized;
    }
}
