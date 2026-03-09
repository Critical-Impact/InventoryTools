using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Compendium.Interfaces;
using Lumina.Excel;

namespace InventoryTools.Compendium.Services;

public interface ICompendiumTypeFactory
{

    ICompendiumType? GetByType(Type type);

    ICompendiumType? GetByType<T>();

    ICompendiumType? GetByRowRef(RowRef rowRef, out Type? rowType);
}

public class CompendiumTypeFactory : ICompendiumTypeFactory
{
    private readonly IEnumerable<ICompendiumType> _compendiumTypes;

    public CompendiumTypeFactory(IEnumerable<ICompendiumType> compendiumTypes)
    {
        _compendiumTypes = compendiumTypes;
    }

    public ICompendiumType? GetByType(Type type)
    {
        return _compendiumTypes.FirstOrDefault(compendiumType => compendiumType.Type == type || (compendiumType.RelatedTypes?.Contains(type) ?? false));
    }

    public ICompendiumType? GetByType<T>()
    {
        return GetByType(typeof(T));
    }

    public ICompendiumType? GetByRowRef(RowRef rowRef, out Type? rowType)
    {
        rowType = null;
        if (rowRef.IsUntyped)
            return null;

        rowType = rowRef.RowType;

        if (rowType == null)
            return null;

        return GetByType(rowType);
    }
}