using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using CSVFile;
using Lumina;
using LuminaSupplemental.Excel.Model;
using Sylvan.Data.Csv;

namespace InventoryTools.Services;

public class ContainerAwareCsvLoader
{
    private readonly IComponentContext _componentContext;
    private readonly GameData _gameData;

    public ContainerAwareCsvLoader(IComponentContext componentContext, GameData gameData)
    {
        _componentContext = componentContext;
        _gameData = gameData;
    }

    public bool ToCsvRaw<T>(List<T> items, string filePath) where T : ICsv
    {
        try
        {
            using (StreamWriter dest = new StreamWriter(filePath))
            {
                CSVWriter csvWriter = new CSVWriter(dest);
                foreach (T obj in items)
                {
                    if (obj.IncludeInCsv())
                        csvWriter.WriteLine((IEnumerable<object>) obj.ToCsv());
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public List< T > LoadCsv<T>(string filePath, out List<string> failedLines, out List<Exception> exceptions) where T : ICsv
    {
        failedLines = new List< string >();
        exceptions = new List< Exception >();
        var items = new List< T >();

        using CsvDataReader dr = CsvDataReader.Create(filePath, new CsvDataReaderOptions(){HasHeaders = false});
        while (dr.Read())
        {
            string[] fields = new string[dr.FieldCount];

            for(int i = 0; i < dr.FieldCount; i++)
            {
                fields[i] = dr.GetString(i);
            }
            T item = _componentContext.Resolve<T>();
            try
            {
                item.FromCsv( fields );
                if( _gameData != null )
                {
                    item.PopulateData( _gameData.Excel, _gameData.Options.DefaultExcelLanguage );
                }
                items.Add( item );
            }
            catch( Exception e )
            {
                exceptions.Add(e);
                failedLines.Add( String.Join( ",",fields ) );
            }
        }
        return items;
    }
}