using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost(
        "/api/convert",
        async (IFormFile file) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest("請上傳一個檔案。");

            var fileName = file.FileName.ToLower();
            if (!fileName.EndsWith(".doc") && !fileName.EndsWith(".docx"))
                return Results.BadRequest("目前只支援 .doc 或 .docx 檔案。");

            // 每個請求用獨立的工作目錄，避免多個請求同時搶用 LibreOffice 使用者設定檔而互相鎖住
            var jobId = Guid.NewGuid().ToString();
            var workDir = Path.Combine(Path.GetTempPath(), "docx-preview", jobId);
            Directory.CreateDirectory(workDir);

            var inputPath = Path.Combine(workDir, file.FileName);
            var userProfileDir = Path.Combine(workDir, "lo-profile");

            try
            {
                using (var stream = File.Create(inputPath))
                {
                    await file.CopyToAsync(stream);
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "soffice",
                    Arguments =
                        $"--headless "
                        + $"-env:UserInstallation=file://{userProfileDir} "
                        + $"--convert-to pdf "
                        + $"--outdir \"{workDir}\" \"{inputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };

                using var process = Process.Start(psi)!;
                var stdErrTask = process.StandardError.ReadToEndAsync();
                var completed = process.WaitForExit(30000); // 最多等 30 秒

                if (!completed)
                {
                    process.Kill(true);
                    return Results.Problem("轉換逾時，請確認檔案是否過大或內容異常。");
                }

                if (process.ExitCode != 0)
                {
                    var stdErr = await stdErrTask;
                    return Results.Problem("轉換失敗：" + stdErr);
                }

                var pdfFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
                var pdfPath = Path.Combine(workDir, pdfFileName);

                if (!File.Exists(pdfPath))
                    return Results.Problem("轉換完成但找不到輸出的 PDF 檔案。");

                var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                return Results.File(pdfBytes, "application/pdf", pdfFileName);
            }
            finally
            {
                // 清理暫存檔案，避免容器內磁碟空間持續累積
                try
                {
                    Directory.Delete(workDir, true);
                }
                catch { }
            }
        }
    )
    .DisableAntiforgery();

app.Run();
