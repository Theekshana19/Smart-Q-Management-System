// Must match Visual Studio launch profile (see backend SmartQ.API/Properties/launchSettings.json)
// https profile (default in VS): port 7287
// http profile: port 5105 → use http://localhost:5105/api
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7287/api',
  hubUrl: 'https://localhost:7287/hubs/queue'
};
