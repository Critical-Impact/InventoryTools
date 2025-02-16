using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using CSVFile;
using Lumina;
using LuminaSupplemental.Excel.Model;

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
        using var fileStream = new FileStream( filePath, FileMode.Open );
        using( StreamReader reader = new StreamReader( fileStream ) )
        {
            failedLines = new List< string >();
            exceptions = new List< Exception >();
            var items = new List< T >();

            if( reader.EndOfStream )
            {
                return items;
            }

            FileInfo f = new FileInfo(filePath);
            var fileContents = reader.ReadToEnd();
            fileContents = fileContents.ReplaceLineEndings("\n");
            var csvReader = CSVFile.CSVReader.FromString( fileContents, new CSVSettings { Encoding = Encoding.UTF8, LineSeparator = "\n", BufferSize = (int)f.Length} ); //BufferSize fixes a infinite loop
            foreach( var line in csvReader.Lines() )
            {
                T item = _componentContext.Resolve<T>();
                try
                {
                    item.FromCsv( line );
                    if( _gameData != null)
                    {
                        item.PopulateData( _gameData.Excel, _gameData.Options.DefaultExcelLanguage );
                    }
                    items.Add( item );
                }
                catch( Exception e )
                {
                    exceptions.Add(e);
                    failedLines.Add( String.Join( ",",line ) );
                }
            }
            return items;
        }
    }
}