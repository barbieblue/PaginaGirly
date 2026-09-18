Add-Type -AssemblyName System.Net.Http

$subastaId = 8
$url = "http://localhost:5137/api/Subastas/$subastaId/bids"

$body1 = '{"usuarioId": 2, "monto": 1000}'
$body2 = '{"usuarioId": 3, "monto": 1000}'

$client = New-Object System.Net.Http.HttpClient

$content1 = New-Object System.Net.Http.StringContent($body1, [System.Text.Encoding]::UTF8, "application/json")
$content2 = New-Object System.Net.Http.StringContent($body2, [System.Text.Encoding]::UTF8, "application/json")

$task1 = $client.PostAsync($url, $content1)
$task2 = $client.PostAsync($url, $content2)

[System.Threading.Tasks.Task]::WaitAll($task1, $task2)

$resultado1 = $task1.Result.Content.ReadAsStringAsync().Result
$resultado2 = $task2.Result.Content.ReadAsStringAsync().Result

Write-Host "=== Peticion 1 (usuario 2) ==="
Write-Host "Status: $($task1.Result.StatusCode)"
Write-Host $resultado1
Write-Host ""
Write-Host "=== Peticion 2 (usuario 3) ==="
Write-Host "Status: $($task2.Result.StatusCode)"
Write-Host $resultado2