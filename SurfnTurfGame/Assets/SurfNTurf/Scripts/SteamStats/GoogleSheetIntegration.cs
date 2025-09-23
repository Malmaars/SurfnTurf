using UnityEngine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;

public class GoogleSheetsIntegration : MonoBehaviour
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string ApplicationName = "SteamSheetsService";
    private static readonly string SheetId = "1C5pegabmA4-mfRjY3AfhPpEyUpuD2hbuD-koaB8QUS4";
    private static readonly string SheetName = "Sheet1";
    private SheetsService sheetsService;
    public static GoogleSheetsIntegration instance;

    void Awake()
    {
        AuthenticateGoogleSheets();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void AuthenticateGoogleSheets()
    {
        string credentialsPath = Path.Combine(Application.streamingAssetsPath, "steamsta-95b2958f8673.json");

        if (!File.Exists(credentialsPath))
        {
            Debug.LogError("❌ Service account JSON not found!");
            return;
        }

        GoogleCredential credential;
        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        Debug.Log("✅ Google Sheets API authenticated!");
    }

    public async Task StoreSteamID(string steamID)
    {
        if (sheetsService == null)
        {
            Debug.LogError("❌ Google Sheets API is not authenticated!");
            return;
        }

        string personaName = SteamFriends.GetPersonaName().ToString();

        // Read existing Steam IDs
        var range = $"{SheetName}!A:B"; // Columns A and B
        var getRequest = sheetsService.Spreadsheets.Values.Get(SheetId, range);
        var response = await getRequest.ExecuteAsync();

        if (response.Values != null)
        {
            for (int i = 0; i < response.Values.Count; i++)
            {
                var row = response.Values[i];
                if (row.Count > 0 && row[0].ToString() == steamID)
                {
                    // Update the persona name in column B
                    var updateRange = $"{SheetName}!B{i + 1}";
                    var valueRange = new ValueRange
                    {
                        Values = new List<IList<object>> { new List<object> { personaName } }
                    };

                    var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, SheetId, updateRange);
                    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                    try
                    {
                        await updateRequest.ExecuteAsync();
                        Debug.Log($"✅ Updated persona name for Steam ID {steamID} at row {i + 1}.");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("❌ Error updating persona name: " + e.Message);
                    }

                    return; // Exit after updating
                }
            }
        }

        // Find next empty row
        int nextRow = (response.Values != null) ? response.Values.Count + 1 : 2; // Start at row 2
        var insertRange = $"{SheetName}!A{nextRow}:B{nextRow}";

        var valueRangeNew = new ValueRange
        {
            Values = new List<IList<object>> { new List<object> { steamID, personaName } }
        };

        var insertRequest = sheetsService.Spreadsheets.Values.Update(valueRangeNew, SheetId, insertRange);
        insertRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        try
        {
            await insertRequest.ExecuteAsync();
            Debug.Log($"✅ Steam ID {steamID} and persona name {personaName} stored at row {nextRow}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error storing Steam ID and persona name: " + e.Message);
        }
    }

    public async void StoreStat(string statName, int statValue)
    {
        if (sheetsService == null)
        {
            Debug.LogError("❌ Google Sheets API is not authenticated!");
            return;
        }

        string steamID = SteamUser.GetSteamID().ToString();

        // Read the whole sheet
        var range = $"{SheetName}";
        var getRequest = sheetsService.Spreadsheets.Values.Get(SheetId, range);
        var response = await getRequest.ExecuteAsync();

        if (response.Values == null || response.Values.Count == 0)
        {
            Debug.LogError("❌ Sheet is empty!");
            return;
        }

        // Find stat column index (first row is header, stats start at column C = index 2)
        int statCol = -1;
        var header = response.Values[0];
        for (int i = 2; i < header.Count; i++) // Start at index 2
        {
            if (header[i].ToString() == statName)
            {
                statCol = i;
                break;
            }
        }

        // If stat column doesn't exist, add it
        if (statCol == -1)
        {
            statCol = header.Count;
            header.Add(statName);

            var headerRange = $"{SheetName}!A1:{ColumnLetter(statCol)}1";
            var headerValueRange = new ValueRange { Values = new List<IList<object>> { header } };
            var updateHeaderRequest = sheetsService.Spreadsheets.Values.Update(headerValueRange, SheetId, headerRange);
            updateHeaderRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await updateHeaderRequest.ExecuteAsync();
        }

        // Find row with SteamID
        int steamRow = -1;
        for (int i = 1; i < response.Values.Count; i++)
        {
            var row = response.Values[i];
            if (row.Count > 0 && row[0].ToString() == steamID)
            {
                steamRow = i;
                break;
            }
        }

        // If SteamID row doesn't exist, add it
        if (steamRow == -1)
        {
            steamRow = response.Values.Count;
            var newRow = new List<object>();
            for (int i = 0; i <= statCol; i++)
                newRow.Add(""); // Fill with empty cells
            newRow[0] = steamID;
            response.Values.Add(newRow);

            var insertRange = $"{SheetName}!A{steamRow + 1}:{ColumnLetter(statCol)}{steamRow + 1}";
            var insertValueRange = new ValueRange { Values = new List<IList<object>> { newRow } };
            var insertRequest = sheetsService.Spreadsheets.Values.Update(insertValueRange, SheetId, insertRange);
            insertRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await insertRequest.ExecuteAsync();
        }

        // Update the stat value in the correct cell
        var cellRange = $"{SheetName}!{ColumnLetter(statCol)}{steamRow + 1}";
        var cellValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { statValue } } };
        var cellUpdateRequest = sheetsService.Spreadsheets.Values.Update(cellValueRange, SheetId, cellRange);
        cellUpdateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        await cellUpdateRequest.ExecuteAsync();

        Debug.Log($"✅ Stored stat '{statName}' with value {statValue} for SteamID {steamID} at row {steamRow + 1}, column {statCol + 1}.");
    }
    public async void StoreStat(string statName, string statValue)
    {
        if (sheetsService == null)
        {
            Debug.LogError("❌ Google Sheets API is not authenticated!");
            return;
        }

        string steamID = SteamUser.GetSteamID().ToString();

        // Read the whole sheet
        var range = $"{SheetName}";
        var getRequest = sheetsService.Spreadsheets.Values.Get(SheetId, range);
        var response = await getRequest.ExecuteAsync();

        if (response.Values == null || response.Values.Count == 0)
        {
            Debug.LogError("❌ Sheet is empty!");
            return;
        }

        // Find stat column index (first row is header, stats start at column C = index 2)
        int statCol = -1;
        var header = response.Values[0];
        for (int i = 2; i < header.Count; i++) // Start at index 2
        {
            if (header[i].ToString() == statName)
            {
                statCol = i;
                break;
            }
        }

        // If stat column doesn't exist, add it
        if (statCol == -1)
        {
            statCol = header.Count;
            header.Add(statName);

            var headerRange = $"{SheetName}!A1:{ColumnLetter(statCol)}1";
            var headerValueRange = new ValueRange { Values = new List<IList<object>> { header } };
            var updateHeaderRequest = sheetsService.Spreadsheets.Values.Update(headerValueRange, SheetId, headerRange);
            updateHeaderRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await updateHeaderRequest.ExecuteAsync();
        }

        // Find row with SteamID
        int steamRow = -1;
        for (int i = 1; i < response.Values.Count; i++)
        {
            var row = response.Values[i];
            if (row.Count > 0 && row[0].ToString() == steamID)
            {
                steamRow = i;
                break;
            }
        }

        // If SteamID row doesn't exist, add it
        if (steamRow == -1)
        {
            steamRow = response.Values.Count;
            var newRow = new List<object>();
            for (int i = 0; i <= statCol; i++)
                newRow.Add(""); // Fill with empty cells
            newRow[0] = steamID;
            response.Values.Add(newRow);

            var insertRange = $"{SheetName}!A{steamRow + 1}:{ColumnLetter(statCol)}{steamRow + 1}";
            var insertValueRange = new ValueRange { Values = new List<IList<object>> { newRow } };
            var insertRequest = sheetsService.Spreadsheets.Values.Update(insertValueRange, SheetId, insertRange);
            insertRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await insertRequest.ExecuteAsync();
        }

        // Update the stat value in the correct cell
        var cellRange = $"{SheetName}!{ColumnLetter(statCol)}{steamRow + 1}";
        var cellValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { statValue } } };
        var cellUpdateRequest = sheetsService.Spreadsheets.Values.Update(cellValueRange, SheetId, cellRange);
        cellUpdateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        await cellUpdateRequest.ExecuteAsync();

        Debug.Log($"✅ Stored stat '{statName}' with value {statValue} for SteamID {steamID} at row {steamRow + 1}, column {statCol + 1}.");
    }

    // Helper to convert column index to Excel column letter (supports up to ZZ)
    private string ColumnLetter(int index)
    {
        string col = "";
        while (index >= 0)
        {
            col = (char)('A' + (index % 26)) + col;
            index = index / 26 - 1;
        }
        return col;
    }
}
