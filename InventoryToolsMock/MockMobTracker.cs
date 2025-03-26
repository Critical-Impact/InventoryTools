using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using LuminaSupplemental.Excel.Model;

namespace InventoryToolsMock;

public class MockMobTracker : IMobTracker
{
    public void Enable()
    {
        _enabled = true;
    }
    
    private bool _enabled;

    public bool Enabled => _enabled;

    public void Disable()
    {
        _enabled = false;
    }

    public void Dispose()
    {
    }

    public bool SaveCsv(string filePath, List<MobSpawnPosition> positions)
    {
        using var fileStream = new FileStream( filePath, FileMode.Create );
        using( StreamWriter reader = new StreamWriter( fileStream ) )
        {
            try
            {
                using var csvReader = new CSVFile.CSVWriter( reader );
                csvReader.WriteLine(MobSpawnPosition.GetHeaders());
                foreach( var position in positions )
                {
                    var linePosition = position.ToCsv( );
                    csvReader.WriteLine(linePosition);
                }
                    
                return true;
            }
            catch( Exception e )
            {
                return false;
            }
        }
    }

    public List<MobSpawnPosition> LoadCsv(string filePath, out bool success)
    {
        success = false;
        if (File.Exists(filePath))
        {
            using var fileStream = new FileStream(filePath, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                try
                {
                    var csvReader = CSVFile.CSVReader.FromString(reader.ReadToEnd());
                    var items = new List<MobSpawnPosition>();
                    foreach (var line in csvReader.Lines())
                    {
                        MobSpawnPosition item = new MobSpawnPosition();
                        item.FromCsv(line);
                        items.Add(item);
                    }

                    success = true;
                    return items;
                }
                catch (Exception e)
                {
                    success = false;
                    return new List<MobSpawnPosition>();
                }
            }
        }

        return new List<MobSpawnPosition>();
    }
    
    private bool WithinRange(Vector3 pointA, Vector3 pointB, float maxRange)
    {
        RectangleF recA = new RectangleF( new PointF(pointA.X - maxRange, pointA.Y - maxRange), new SizeF(maxRange,maxRange));
        RectangleF recB = new RectangleF( new PointF(pointB.X - maxRange, pointB.Y - maxRange), new SizeF(maxRange,maxRange));
        return recA.IntersectsWith(recB);
    }
    
    private const float maxRange = 20.0f;

    private Dictionary<uint, Dictionary<uint, List<MobSpawnPosition>>> positions = new Dictionary<uint, Dictionary<uint, List<MobSpawnPosition>>>();

    public void AddEntry(MobSpawnPosition spawnPosition)
    {
        positions.TryAdd(spawnPosition.TerritoryTypeId, new Dictionary<uint, List<MobSpawnPosition>>());
        positions[spawnPosition.TerritoryTypeId].TryAdd(spawnPosition.BNpcNameId, new List<MobSpawnPosition>());
        //Store 
        var existingPositions = positions[spawnPosition.TerritoryTypeId][spawnPosition.BNpcNameId];
        if (!existingPositions.Any(c => WithinRange(spawnPosition.Position, c.Position, maxRange)))
        {
            existingPositions.Add(spawnPosition);
        }
    }

    public void SetEntries(List<MobSpawnPosition> spawnPositions)
    {
        foreach (var spawn in spawnPositions)
        {
            AddEntry(spawn);
        }
    }

    public List<MobSpawnPosition> GetEntries()
    {
        Disable();
        var newPositions = positions.SelectMany(c => c.Value.SelectMany(d => d.Value.Select(e => e))).ToList();
        Enable();
        return newPositions;
    }

    public void ClearSavedData()
    {
    }
}