這是我這次面試的後端技術測試專案。主要邏輯是用 .NET 8 寫一個 Web API 來對 MSSQL 做 CRUD 操作。

專案架構
MSSQL -> ASP.NET Core Web API (Dapper)

開發與執行步驟
1. 建立 SQL DB
使用專案根目錄附帶的 Myoffice_ACPD.bak 進行資料庫還原。

還原後確認資料庫名稱為 Myoffice_ACPD。

專案內已整合提供的 NEWSID 預存程序來產生主鍵。

2. 建立 Web API 專案
採用 .NET 8 Web API 範本。

使用 Dapper 實作 RESTful API，包含 GET、POST、PUT、DELETE。

特別處理：在存檔（POST/PUT）時加入了 Email 不得重複的檢查邏輯。

3. 發布與版本管理
專案已發布至 GitHub。

遵循 Git 開發流程：

建立 feature/crud-implementation 分支進行功能開發。

開發完成後合併至 develop 分支進行整合。

最後合併回 main 分支作為最終繳交版本。

已加入 .gitignore 排除 .vs、bin、obj 等編譯暫存檔。

4. 測試方式
直接按 F5 執行專案，會自動跳出 Swagger 介面。

Swagger 內已附帶測試用的 JSON 範例資料，可直接進行 CRUD 測試。
