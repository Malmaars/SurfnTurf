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
}
