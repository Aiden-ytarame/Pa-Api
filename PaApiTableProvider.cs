using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PaApi;

public class PaApiTableProvider : ITableProvider
{
    public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale) where TTable : LocalizationTable
    {
        if (typeof(TTable) == typeof(StringTable) && (tableCollectionName == Plugin.Guid || SettingsHelper.ModSettingsDefinitions.ContainsKey(tableCollectionName)))
        {
            var table = ScriptableObject.CreateInstance<StringTable>();
            table.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
            table.SharedData.TableCollectionName = tableCollectionName;
            table.LocaleIdentifier = locale.Identifier;
            
            return Addressables.ResourceManager.CreateCompletedOperation(table as TTable, null);
        }
        
        return default;
    }
}