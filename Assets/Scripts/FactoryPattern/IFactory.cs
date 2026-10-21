using UnityEngine;

public interface IFactory
{
    //Jumlah jenis object yang dapat dibuat
    int Count { get; }

    GameObject FactoryMethod(int tag);
}
