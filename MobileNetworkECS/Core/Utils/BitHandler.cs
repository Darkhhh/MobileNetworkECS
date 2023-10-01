namespace MobileNetworkECS.Core.Utils;

public static class BitHandler
{
    public static uint SetBit(uint val, int index, bool bit)
    {
        if (index is < 0 or > 32) throw new Exception($"Incorrect bit index {index}");                      
        
        uint tempValue = 1;   
        tempValue <<= index; // устанавливаем нужный бит в единицу   
        val &= ~tempValue; // сбрасываем в 0 нужный бит             
        if (bit) // если бит требуется установить в 1             
        {                
            val |= tempValue; // то устанавливаем нужный бит в 1             
        } 
        return val;  
    } 
    public static bool GetBit(uint val, int index) 
    {           
        if (index is > 32 or < 0) throw new Exception($"Incorrect bit index {index}");
        return ((val>>index)&1)>0; // собственно все вычисления 
    }  
}