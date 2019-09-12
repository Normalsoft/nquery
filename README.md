# nquery

**A new kind of desktop database.**

nquery provides a multi-user desktop database via a chat bot, currently accessible via Discord. It uses SQLite and MoonSharp to allow chat room administrators to build small, interactive, reusable custom tools.

Runs on .NET Core 2.2.

## Roadmap

### Essentials

- [x] Basic SQLite open session handling (auto connect and manual disconnect)
- [x] Basic data manipulation and queries with database per-group (Discord guild)
- [x] Query storage and recall with per-recall parameters
- [ ] Recall queries with white-labeled commands
- [ ] Lua script storage and recall with data and text reply API
- [ ] Lua script event hooks (including per-message emoji reactions)
- [ ] Scheduled and/or per-session backup and restore (only owner may delete backups)
- [ ] Automatically disconnect SQLite sessions from inactivity

### Extras

- [x] Command to generate messages with [Rant](https://github.com/TheBerkin/rant)
- [ ] Attach output as TXT/CSV file
- [ ] Attach output as Excel file with stylized table
- [ ] Lua script external REST/JSON API consumer
- [ ] Allow PostgreSQL instead of SQLite
- [ ] Support for Microsoft Teams
- [ ] PowerShell Core script storage, recall, and event hooks (with remoting)
- [ ] Other bots (i.e. VoIP music players) allowing interoperability with nquery

## Commands for Discord

*This should show what is currently implemented.*

| Command                           | Description                                                                                                           |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `nq q <opts> <...query>`          | Generic query command to run any type of query with normal Discord-related options (shown below).                     |
| `nq (tq,sq,cq) <...query>`        | Query with pre-filled `type` being `table`, `scalar`, or `csv` respectively.                                          |
| `nq dc`                           | Disconnect the guild's SQLite session. This frees memory and ensures the database is written entirely to disk.        |
| `nq co`                           | Connect the guild's SQLite session. Loads the database from disk. Done implicitly upon any query.                     |
| `nq dbn`                          | Show's the internal filename of the database and other info.                                                          |
| `nq qs <name> <opts> <...query>`  | Store a query, optionally specifying options to be invoked when recalled.                                             |
| `nq qr <name> <opts> <...params>` | Recall and run a query. Optionally override stored options, otherwise all further parameters are passed to the query. |
| `nq ql`                           | List stored queries for the guild.                                                                                    |
| `nq qd <name>`                    | Delete a stored query.                                                                                                |

`opts` is text (optionally quotable) recognized similarly to URL query parameters, i.e. `?type=table&nowrap` (where no `=` means `nowrap=true`). `opts` will generally be folded into the rest of the command (e.g. the body of your SQL query) if it does not begin with a question mark.

To use parameters provided in a query recall, store your query with keywords `@0` being the entire remaining message in one string of text, or `@1` `@2` `@3`, etc, being space-separated words or quoted strings of text in order. e.g. `nq qs age-of SELECT age FROM people WHERE fname = @1 AND lname = @2` and `nq qr age-of John Doe`

### Options for queries in Discord

*Used in running queries, saving and recalling.*

| Option   | Accepted values          | Default | Description                                                                                                                                              |
| -------- | ------------------------ | ------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `type`   | `table`, `scalar`, `csv` | `table` | Specifies how the text should appear when sent; either a Markdown-formatted table, a single value (first column of first row), or a csv-formatted table. |
| `nowrap` | `true` or none           | none    | Specifies that the text should not be wrapped in a code block for monospaced text. Output will be sent as an unformatted message.                        |

### Example in Discord

![image](https://user-images.githubusercontent.com/37567272/64517669-6b719f00-d2b6-11e9-9d32-468cb23f12b9.png)
