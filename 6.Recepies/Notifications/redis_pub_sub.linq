<Query Kind="Program">
  <NuGetReference>StackExchange.Redis</NuGetReference>
  <Namespace>StackExchange.Redis</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

void Main()
{
	var redis = ConnectionMultiplexer.Connect("127.0.0.1");

	var payload = JsonSerializer.Serialize(new { ForUserId = "foo", Message = "hello world" });

	redis.GetSubscriber().Publish("notification", payload);
}
