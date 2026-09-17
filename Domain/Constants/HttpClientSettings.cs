namespace Domain.Constants;

internal class HttpClientSettings
{
    public const string ARCHIDEKT_BASE_URL = "https://archidekt.com/api/";
    public const string MOXFIELD_BASE_URL = "https://api2.moxfield.com/v3/";
    public const string EDHREC_BASE_URL = "https://edhrec.com/";
    public const string GOLDFISH_BASE_URL = "https://www.mtggoldfish.com/";
    public const string CUBECOBRA_BASE_URL = "https://www.cubecobra.com/";
    public const string SCRYFALL_BASE_URL = "https://api.scryfall.com/";

    public const int DEFAULT_TIMEOUT_SECONDS = 30;

    public const string APP_NAME = "MagicProxyPrinter";
    public const string ACCEPT_HTML = "text/html";
    public const string ACCEPT_JSON = "text/json";
}
