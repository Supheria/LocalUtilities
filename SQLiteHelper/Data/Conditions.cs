using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalUtilities.TypeToolKit.Text;
using static LocalUtilities.SQLiteHelper.Data.Conditions;

namespace LocalUtilities.SQLiteHelper.Data;

public class Conditions : IList<Condition>
{
    public enum Combos
    {
        Or,
        And,
    }

    public Keywords Combo { get; }
    
    public List<Condition> ConditionList { get; } = [];

    public int Count => ConditionList.Count;

    public bool IsReadOnly => true;

    public Condition this[int index]
    {
        get => ConditionList[index];
        set => ConditionList[index] = value;
    }

    public Conditions(Combos combo, params Condition[] conditions)
    {
        Combo = combo switch
        {
            Combos.Or => Keywords.Or,
            Combos.And => Keywords.And,
            _ => Keywords.Or
        };
        ConditionList.AddRange(conditions);
    }

    public Conditions(Condition condition)
    {
        Combo = Keywords.Or;
        ConditionList = [condition];
    }

    public Conditions()
    {
        Combo = Keywords.Or;
        ConditionList = [];
    }

    public int IndexOf(Condition item)
    {
        return ConditionList.IndexOf(item);
    }

    public void Insert(int index, Condition item)
    {
        ConditionList.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        ConditionList.RemoveAt(index);
    }

    public void Add(Condition item)
    {
        ConditionList.Add(item);
    }

    public void Clear()
    {
        ConditionList.Clear();
    }

    public bool Contains(Condition item)
    {
        return ConditionList.Contains(item);
    }

    public void CopyTo(Condition[] array, int arrayIndex)
    {
        ConditionList.CopyTo(array, arrayIndex);
    }

    public bool Remove(Condition item)
    {
        return ConditionList.Remove(item);
    }

    public IEnumerator<Condition> GetEnumerator()
    {
        return ConditionList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ConditionList.GetEnumerator();
    }
}
