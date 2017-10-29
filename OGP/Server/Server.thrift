namespace csharp OGP.Server

struct ServerDefinition
{
  1: list<string> SupportedGames,
  2: i32 TickDuration,
  3: i32 NumPlayers
}

service ClusterService
{
  ServerDefinition GetDefinition();
}
