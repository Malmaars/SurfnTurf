using UnityEngine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GoogleSheetsIntegration : MonoBehaviour
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string ApplicationName = "SteamSheetsService";
    private static readonly string SheetId = "1C5pegabmA4-mfRjY3AfhPpEyUpuD2hbuD-koaB8QUS4";
    private static readonly string SheetName = "Sheet1";
    private SheetsService sheetsService;

    void Start()
    {
        AuthenticateGoogleSheets();
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

        // Read existing Steam IDs
        var range = $"{SheetName}!A:A"; // Entire A column
        var getRequest = sheetsService.Spreadsheets.Values.Get(SheetId, range);
        var response = await getRequest.ExecuteAsync();

        if (response.Values != null)
        {
            foreach (var row in response.Values)
            {
                if (row.Count > 0 && row[0].ToString() == steamID)
                {
                    Debug.Log($"✅ Steam ID {steamID} already exists. No action taken.");
                    return; // Exit if ID already exists
                }
            }
        }

        // Find next empty row
        int nextRow = (response.Values != null) ? response.Values.Count + 1 : 2; // Start at row 2
        var insertRange = $"{SheetName}!A{nextRow}";

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { new List<object> { steamID } }
        };

        var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, SheetId, insertRange);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        try
        {
            await updateRequest.ExecuteAsync();
            Debug.Log($"✅ Steam ID {steamID} stored at row {nextRow}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error storing Steam ID: " + e.Message);
        }
    }
}
