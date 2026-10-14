# DocxPreviewApi — .doc/.docx 轉 PDF 預覽服務

[English](README.md) | 繁體中文

一個以 **.NET 10 (ASP.NET Core Minimal API)** 實作的小型模組，將 `.doc` 與 `.docx` 檔轉換為 PDF 並呈現在網頁上，轉換引擎使用 **LibreOffice**。前端位於 `http://localhost:7090/`，以 **Docker Compose** 啟動。

## 專案背景

許多網頁應用需要預覽 Word 文件，而不希望使用者必須先下載檔案。這個模組示範了一個精簡且可運作的作法：伺服器端將文件轉換為 PDF，瀏覽器再以 PDF 顯示。

## 技術棧

| 類別 | 使用技術 |
|---|---|
| 後端框架 | ASP.NET Core 10 (Minimal API) |
| 前端 | 由 `wwwroot/` 提供的單頁 HTML/JS |
| 文件轉換 | LibreOffice (`soffice`, headless) |
| 字型 | Noto CJK、Liberation（處理中日韓文與一般文字） |
| 容器化 | Docker、Docker Compose |
| Port | 7090 |

## 功能特色

- **單一端點，`POST /api/convert`**：接受 `.doc` 或 `.docx` 檔，回傳 PDF。
- **前端預覽**：示範頁（由 `wwwroot/` 提供）上傳檔案後，以 `<iframe>` 顯示回傳的 PDF。
- **每個請求獨立的工作目錄**：每次轉換都在獨立的暫存資料夾中執行，並使用各自的 LibreOffice 使用者設定檔，避免多個並發請求互相鎖定。
- **逾時與錯誤處理**：轉換超過 30 秒會被強制終止；LibreOffice 的錯誤訊息也會回傳給呼叫端。
- **自動清理**：每次請求後自動刪除暫存檔案與 LibreOffice 設定檔目錄。

## API

| 方法 | 路徑 | 說明 |
|---|---|---|
| POST | `/api/convert` | 上傳 `.doc` 或 `.docx` 檔，回傳 PDF |

回傳：
- 成功：`200`，PDF 檔（`application/pdf`）。
- 未上傳檔案或副檔名非 `.doc` / `.docx`：`400`。
- 轉換失敗或逾時：`500`（ProblemDetails）。

## 啟動方式

1. 建置並啟動容器：

```bash
docker compose up --build
```

2. 開啟 `http://localhost:7090/`。

3. 選擇 `.doc` 或 `.docx` 檔。頁面會將其轉換，並直接以 PDF 顯示。

## 運作方式

1. 前端以 `multipart/form-data` 上傳檔案到 `POST /api/convert`。
2. 伺服器將上傳的檔案寫入該請求專屬的暫存目錄。
3. 在 headless 模式下啟動 LibreOffice，使用獨立使用者設定檔，將檔案轉換為 PDF。
4. 伺服器將 PDF 位元組回傳給瀏覽器，並以 `<iframe>` 顯示。

## 已知限制

- 轉換為同步且 CPU-bound，過大的檔案會占用一個請求處理槽直到觸發 30 秒逾時。
- 沒有身份驗證與速率限制。此模組設計目的是嵌入到另一個 API 之後，不應直接對外曝光。

## License

MIT
