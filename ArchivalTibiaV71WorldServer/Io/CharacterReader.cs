using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer.Io
{
    public class CharacterReader
    {
        private ref struct Result<T>
        {
            public T Value;
            public bool Success;

            private Result(bool success)
            {
                Value = default;
                Success = false;
            }

            private Result(T value)
            {
                Value = value;
                Success = true;
            }

            public static Result<T> Ok(T value)
            {
                return new Result<T>(value);
            }

            public static Result<T> Error() => new Result<T>(false);
        }

        private class Token
        {
            public readonly string StringText;
            public readonly Tokens Type;
            public readonly long Number;
            public static readonly Token Eof = new Token(Tokens.Eof);
            public static readonly Token Unknown = new Token(Tokens.Unknown);
            public static readonly Token None = new Token(Tokens.None);
            public static readonly Token LeftParenthesis = new Token(Tokens.LeftParenthesis);
            public static readonly Token RightParenthesis = new Token(Tokens.RightParenthesis);
            public static readonly Token EqualsToken = new Token(Tokens.Equals);
            public static readonly Token LeftBrace = new Token(Tokens.LeftBrace);
            public static readonly Token RightBrace = new Token(Tokens.RightBrace);
            public static readonly Token Comma = new Token(Tokens.Comma);
            public readonly static Token LeftBracket = new Token(Tokens.LeftBracket);
            public readonly static Token RightBracket = new Token(Tokens.RightBracket);

            public Token(Tokens type)
            {
                Type = type;
            }

            public Token(string stringText)
            {
                StringText = stringText;
                Type = Tokens.String;
            }

            public Token(long number)
            {
                Number = number;
                Type = Tokens.Number;
            }
        }

        private ref struct PlayerLexer
        {
            private static Dictionary<string, Token> _keywords;

            static PlayerLexer()
            {
                _keywords = new Dictionary<string, Token>
                {
                    {"Id", new Token(Tokens.Id)},
                    {"Name", new Token(Tokens.Name)},
                    {"Password", new Token(Tokens.Password)},
                    {"Position", new Token(Tokens.Position)},
                    {"TemplePosition", new Token(Tokens.TemplePosition)},
                    {"Outfit", new Token(Tokens.Outfit)},
                    {"Level", new Token(Tokens.Level)},
                    {"Experience", new Token(Tokens.Experience)},
                    {"Health", new Token(Tokens.Health)},
                    {"MaxHealth", new Token(Tokens.MaxHealth)},
                    {"Mana", new Token(Tokens.Mana)},
                    {"MaxMana", new Token(Tokens.MaxMana)},
                    {"Capacity", new Token(Tokens.Capacity)},
                    {"MagicLevel", new Token(Tokens.MagicLevel)},
                    {"MagicLevelExp", new Token(Tokens.MagicLevelExp)},
                    {"Fist", new Token(Tokens.Fist)},
                    {"FistExp", new Token(Tokens.FistExp)},
                    {"Club", new Token(Tokens.Club)},
                    {"ClubExp", new Token(Tokens.ClubExp)},
                    {"Sword", new Token(Tokens.Sword)},
                    {"SwordExp", new Token(Tokens.SwordExp)},
                    {"Axe", new Token(Tokens.Axe)},
                    {"AxeExp", new Token(Tokens.AxeExp)},
                    {"Distance", new Token(Tokens.Distance)},
                    {"DistanceExp", new Token(Tokens.DistanceExp)},
                    {"Shielding", new Token(Tokens.Shielding)},
                    {"ShieldingExp", new Token(Tokens.ShieldingExp)},
                    {"Fishing", new Token(Tokens.Fishing)},
                    {"FishingExp", new Token(Tokens.FishingExp)},
                    {"Equipment", new Token(Tokens.Equipment)},
                    {"Head", new Token(Tokens.Head)},
                    {"Necklace", new Token(Tokens.Necklace)},
                    {"Backpack", new Token(Tokens.Backpack)},
                    {"Armor", new Token(Tokens.Armor)},
                    {"Right", new Token(Tokens.Right)},
                    {"Left", new Token(Tokens.Left)},
                    {"Legs", new Token(Tokens.Legs)},
                    {"Feet", new Token(Tokens.Feet)},
                    {"Ring", new Token(Tokens.Ring)},
                    {"Ammo", new Token(Tokens.Ammo)},
                    {"Inventory", new Token(Tokens.Inventory)},
                    {"Item", new Token(Tokens.Item)},
                    {"Container", new Token(Tokens.Container)},
                    {"IsAdmin", new Token(Tokens.IsAdmin)},
                    {"True", new Token(Tokens.True)},
                    {"False", new Token(Tokens.False)},
                };
            }

            private readonly ReadOnlySpan<char> _chars;
            private int _pos;
            private int _tokenStart;
            private int _length;
            public Token Last;
            public Token Current;

            public PlayerLexer(ReadOnlySpan<char> chars)
            {
                _chars = chars;
                _pos = 0;
                _tokenStart = 0;
                _length = chars.Length;
                Last = Token.None;
                Current = Token.None;
            }

            public Token NextToken()
            {
                if (_pos >= _length)
                    return Token.Eof;
                var token = Token.None;
                while (token.Type == Tokens.None && _pos < _length)
                {
                    switch (_chars[_pos])
                    {
                        case '#':
                            _pos += 1;
                            while (_chars[_pos] != '\n') _pos += 1;
                            continue;
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            _pos += 1;
                            while (_chars[_pos] == ' ' || _chars[_pos] == '\t' || _chars[_pos] == '\r' ||
                                   _chars[_pos] == '\n')
                            {
                                _pos += 1;
                            }

                            continue;
                        case '"':
                            return GetString();
                        case '(':
                            token = Token.LeftParenthesis;
                            break;
                        case ')':
                            token = Token.RightParenthesis;
                            break;
                        case '=':
                            token = Token.EqualsToken;
                            break;
                        case '{':
                            token = Token.LeftBrace;
                            break;
                        case '}':
                            token = Token.RightBrace;
                            break;
                        case '[':
                            token = Token.LeftBracket;
                            break;
                        case ']':
                            token = Token.RightBracket;
                            break;
                        case ',':
                            token = Token.Comma;
                            break;
                        default:
                            if (IsKeywordStart(_chars[_pos]))
                            {
                                return GetKeyword();
                            }
                            else if (IsDigit(_chars[_pos]) || _chars[_pos] == '-')
                            {
                                return GetNumber();
                            }

                            Last = Current;
                            Current = Token.Unknown;
                            return Current;
                    }

                    _pos += 1;
                }


                Last = Current;
                Current = token;

                return token;
            }

            private Token GetKeyword()
            {
                _tokenStart = _pos;
                _pos += 1;
                while (IsKeywordRest(_chars[_pos]) && _pos < _length) _pos += 1;

                if (_keywords.TryGetValue(_chars.Slice(_tokenStart, _pos - _tokenStart).ToString(), out var kw))
                {
                    Last = Current;
                    Current = kw;
                    return Current;
                }

                Last = Current;
                Current = Token.Unknown;
                return Current;
            }

            private bool IsDigit(char c)
            {
                return c >= '0' && c <= '9';
            }

            private Token GetNumber()
            {
                _tokenStart = _pos;
                _pos += 1;
                while (IsDigit(_chars[_pos]) && _pos < _length) _pos += 1;

                Last = Current;
                Current = new Token(long.Parse(_chars.Slice(_tokenStart, _pos - _tokenStart)));
                return Current;
            }

            private bool IsKeywordStart(char c)
            {
                return c >= 'A' && c <= 'Z';
            }

            private bool IsKeywordRest(char c)
            {
                return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
            }

            private Token GetString()
            {
                _pos += 1;
                _tokenStart = _pos;
                var hasEscapeChar = false;
                while (_chars[_pos] != '"' && _pos < _length)
                {
                    if (_chars[_pos] == '\\')
                    {
                        _pos += 1;
                        switch (_chars[_pos])
                        {
                            case '\\':
                            case '"':
                            case 't':
                            case 'n':
                                hasEscapeChar = true;
                                break;
                            default:
                                return Token.Unknown; // unknown escape character
                        }
                    }

                    _pos += 1;
                }

                if (_pos >= _length)
                    return Token.Unknown;


                var charSpan = _chars.Slice(_tokenStart, _pos - _tokenStart);
                _pos += 1;
                if (!hasEscapeChar)
                {
                    Last = Current;
                    Current = new Token(charSpan.ToString());
                    return Current;
                }

                var builder = new StringBuilder();
                for (int i = 0; i < charSpan.Length; i++)
                {
                    if (charSpan[i] == '\\') // we're just going to assume that the validation in the while loop worked
                    {
                        i++;
                        switch (charSpan[i])
                        {
                            case '\\':
                                builder.Append('\\');
                                break;
                            case '"':
                                builder.Append('"');
                                break;
                            case 't':
                                builder.Append('\t');
                                break;
                            case 'n':
                                builder.Append('\n');
                                break;
                            default:
                                return Token.Unknown; // unknown escape character
                        }
                    }
                    else
                    {
                        builder.Append(charSpan[i]);
                    }
                }
                
                Last = Current;
                Current = new Token(builder.ToString());
                return Current;
            }
        }

        public Player Read(string path)
        {
            var fileName = Path.GetFileName(path);
            var read = File.ReadAllText(path);
            var player = new CharacterBuilder();
            var lexer = new PlayerLexer(read.AsSpan());
            Token lastToken = Token.None;
            Token token;
            Result<bool> boolField;
            Result<long> numberField;
            Result<string> stringField;
            Result<Position> posField;
            Result<Outfit> outfitField;
            Result<EquipmentBuilder> equipmentField;
            Result<List<Item>> inventoryField;
            while ((token = lexer.NextToken()).Type != Tokens.Eof && token.Type != Tokens.None)
            {
                switch (token.Type)
                {
                    case Tokens.Unknown:
                        return ReportError(fileName, token, lastToken);
                    case Tokens.Id:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Id = numberField.Value;
                        break;
                    case Tokens.Name:
                        stringField = ParseStringField(ref lexer);
                        if (!stringField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Name = stringField.Value;
                        break;
                    case Tokens.Password:
                        stringField = ParseStringField(ref lexer);
                        if (!stringField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Password = stringField.Value;
                        break;
                    case Tokens.Position:
                        posField = ParsePositionField(ref lexer);
                        if (!posField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Position = posField.Value;
                        break;
                    case Tokens.TemplePosition:
                        posField = ParsePositionField(ref lexer);
                        if (!posField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.TemplePosition = posField.Value;
                        break;
                    case Tokens.Outfit:
                        outfitField = ParseOutfitField(ref lexer);
                        if (!outfitField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Outfit = outfitField.Value;
                        break;
                    case Tokens.Level:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Level = numberField.Value;
                        break;
                    case Tokens.Experience:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Experience = numberField.Value;
                        break;
                    case Tokens.Health:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Hitpoints = numberField.Value;
                        break;
                    case Tokens.MaxHealth:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.MaxHitpoints = numberField.Value;
                        break;
                    case Tokens.Mana:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Mana = numberField.Value;
                        break;
                    case Tokens.MaxMana:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.MaxMana = numberField.Value;
                        break;
                    case Tokens.Capacity:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Capacity = numberField.Value;
                        break;
                    case Tokens.MagicLevel:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.MagicLevel = numberField.Value;
                        break;
                    case Tokens.MagicLevelExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.MagicLevelExp = numberField.Value;
                        break;
                    case Tokens.Fist:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Fist = numberField.Value;
                        break;
                    case Tokens.FistExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.FistExp = numberField.Value;
                        break;
                    case Tokens.Club:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Club = numberField.Value;
                        break;
                    case Tokens.ClubExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.ClubExp = numberField.Value;
                        break;
                    case Tokens.Sword:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Sword = numberField.Value;
                        break;
                    case Tokens.SwordExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.SwordExp = numberField.Value;
                        break;
                    case Tokens.Axe:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Axe = numberField.Value;
                        break;
                    case Tokens.AxeExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.AxeExp = numberField.Value;
                        break;
                    case Tokens.Distance:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Dist = numberField.Value;
                        break;
                    case Tokens.DistanceExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.DistExp = numberField.Value;
                        break;
                    case Tokens.Shielding:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Shield = numberField.Value;
                        break;
                    case Tokens.ShieldingExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.ShieldExp = numberField.Value;
                        break;
                    case Tokens.Fishing:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Fish = numberField.Value;
                        break;
                    case Tokens.FishingExp:
                        numberField = ParseNumberField(ref lexer);
                        if (!numberField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.FishExp = numberField.Value;
                        break;
                    case Tokens.Equipment:
                        equipmentField = ParseEquipmentField(ref lexer);
                        if (!equipmentField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Equipment = equipmentField.Value;
                        break;
                    case Tokens.Inventory:
                        inventoryField = ParseInventoryField(ref lexer);
                        if (!inventoryField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.Inventory = inventoryField.Value;
                        break;
                    case Tokens.IsAdmin:
                        boolField = ParseBoolField(ref lexer);
                        if (!boolField.Success)
                            return ReportError(fileName, token, lastToken);
                        player.IsGm = boolField.Value;
                        break;
                    default:
                        return ReportError(fileName, token, lastToken);
                }

                lastToken = token;
            }

            return player.Build();
        }

        private static Result<List<Item>> ParseInventoryField(ref PlayerLexer lexer)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Equals)
                return Result<List<Item>>.Error();
            if (lexer.NextToken().Type != Tokens.LeftBracket)
                return Result<List<Item>>.Error();

            var items = new List<Item>();
            while ((next = lexer.NextToken()).Type != Tokens.RightBracket && next.Type != Tokens.Eof &&
                   next.Type != Tokens.None && next.Type != Tokens.Unknown)
            {
                switch (next.Type)
                {
                    case Tokens.Item:
                        var item = ParseItemValue(ref lexer);
                        if (!item.Success)
                            return Result<List<Item>>.Error();
                        items.Add(item.Value);
                        break;
                    case Tokens.Container:
                        var container = ParseItemValue(ref lexer);
                        if (!container.Success)
                            return Result<List<Item>>.Error();
                        if (!ParseContainer(ref lexer, container.Value))
                            return Result<List<Item>>.Error();
                        items.Add(container.Value);
                        break;
                    default:
                        return Result<List<Item>>.Error();
                }
            }

            if (next.Type != Tokens.RightBracket)
                return Result<List<Item>>.Error();

            return Result<List<Item>>.Ok(items);
        }

        private static bool ParseContainer(ref PlayerLexer lexer, Item parent)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Equals)
                return false;
            if (lexer.NextToken().Type != Tokens.LeftBrace)
                return false;

            while ((next = lexer.NextToken()).Type != Tokens.RightBrace && next.Type != Tokens.Eof &&
                   next.Type != Tokens.None && next.Type != Tokens.Unknown)
            {
                switch (next.Type)
                {
                    case Tokens.Item:
                        var item = ParseItemValue(ref lexer);
                        if (!item.Success)
                            return false;
                        parent.AddInside(item.Value);
                        break;
                    case Tokens.Container:
                        var container = ParseItemValue(ref lexer);
                        if (!container.Success)
                            return false;
                        ParseContainer(ref lexer, container.Value);
                        parent.AddInside(container.Value);
                        break;
                    default:
                        return false;
                }
            }

            if (next.Type != Tokens.RightBrace)
                return false;

            return true;
        }

        private static Result<EquipmentBuilder> ParseEquipmentField(ref PlayerLexer lexer)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Equals)
                return Result<EquipmentBuilder>.Error();
            if (lexer.NextToken().Type != Tokens.LeftBracket)
                return Result<EquipmentBuilder>.Error();

            var builder = new EquipmentBuilder();
            while ((next = lexer.NextToken()).Type != Tokens.RightBracket && next.Type != Tokens.Eof &&
                   next.Type != Tokens.None && next.Type != Tokens.Unknown)
            {
                if (!ParseEquipmentPiece(ref lexer, builder, next))
                    return Result<EquipmentBuilder>.Error();
            }

            if (next.Type != Tokens.RightBracket)
                return Result<EquipmentBuilder>.Error();

            return Result<EquipmentBuilder>.Ok(builder);
        }

        private static Result<Item> ParseItemValue(ref PlayerLexer lex)
        {
            if (lex.NextToken().Type != Tokens.LeftParenthesis)
                return Result<Item>.Error();
            var value = lex.NextToken();
            if (value.Type != Tokens.Number)
                return Result<Item>.Error();
            var next = lex.NextToken();
            if (next.Type == Tokens.RightParenthesis) // 1 arg
                return Result<Item>.Ok(IoC.Items.Create((ushort) value.Number));

            // 2 args
            if (next.Type != Tokens.Comma)
                return Result<Item>.Error();
            var extra = lex.NextToken();
            if (extra.Type != Tokens.Number)
                return Result<Item>.Error();
            if (lex.NextToken().Type != Tokens.RightParenthesis)
                return Result<Item>.Error();
            return Result<Item>.Ok(IoC.Items.Create((ushort) value.Number, (byte) extra.Number));
        }

        private static bool ParseEquipmentPiece(ref PlayerLexer lexer, EquipmentBuilder builder, Token token)
        {
            var val = ParseItemValue(ref lexer);
            if (!val.Success)
                return false;
            switch (token.Type)
            {
                case Tokens.Head:
                    builder.Head = val.Value;
                    return true;
                case Tokens.Necklace:
                    builder.Necklace = val.Value;
                    return true;
                case Tokens.Backpack:
                    builder.Backpack = val.Value;
                    return true;
                case Tokens.Armor:
                    builder.Armor = val.Value;
                    return true;
                case Tokens.Right:
                    builder.Right = val.Value;
                    return true;
                case Tokens.Left:
                    builder.Left = val.Value;
                    return true;
                case Tokens.Legs:
                    builder.Legs = val.Value;
                    return true;
                case Tokens.Feet:
                    builder.Feet = val.Value;
                    return true;
                case Tokens.Ring:
                    builder.Ring = val.Value;
                    return true;
                case Tokens.Ammo:
                    builder.Ammunition = val.Value;
                    return true;
            }

            return false;
        }

        private static Result<Outfit> ParseOutfitField(ref PlayerLexer lexer)
        {
            if (!MultiValueVerification(ref lexer, Tokens.Outfit)) return Result<Outfit>.Error();
            Token next;
            if ((next = lexer.NextToken()).Type != Tokens.String)
                return Result<Outfit>.Error();
            var name = next.StringText;

            if (!Enum.TryParse<Outfits>(name, true, out var outfit))
                return Result<Outfit>.Error();

            Result<long> longValue;
            if (!(longValue = NextNumberValue(ref lexer)).Success)
                return Result<Outfit>.Error();
            var head = longValue.Value;
            if (!(longValue = NextNumberValue(ref lexer)).Success)
                return Result<Outfit>.Error();
            var body = longValue.Value;
            if (!(longValue = NextNumberValue(ref lexer)).Success)
                return Result<Outfit>.Error();
            var legs = longValue.Value;
            if (!(longValue = NextNumberValue(ref lexer)).Success)
                return Result<Outfit>.Error();
            var feet = longValue.Value;
            if (lexer.NextToken().Type != Tokens.RightParenthesis)
                return Result<Outfit>.Error();

            return Result<Outfit>.Ok(new Outfit(outfit, (byte) head, (byte) body, (byte) legs, (byte) feet));
        }

        private static Result<Position> ParsePositionField(ref PlayerLexer lexer)
        {
            Token next;
            Result<long> longValue;
            if (!MultiValueVerification(ref lexer, Tokens.Position)) return Result<Position>.Error();
            if ((next = lexer.NextToken()).Type != Tokens.Number)
                return Result<Position>.Error();
            var x = next.Number;

            if (!(longValue = NextNumberValue(ref lexer)).Success)
                return Result<Position>.Error();
            var y = longValue.Value;

            if (!(longValue = NextNumberValue(ref lexer)).Success)
                return Result<Position>.Error();
            var z = longValue.Value;
            if (lexer.NextToken().Type != Tokens.RightParenthesis)
                return Result<Position>.Error();
            return Result<Position>.Ok(new Position((ushort) x, (ushort) y, (byte) z));
        }

        private static Result<long> NextNumberValue(ref PlayerLexer lexer)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Comma)
                return Result<long>.Error();
            if ((next = lexer.NextToken()).Type != Tokens.Number)
                return Result<long>.Error();
            return Result<long>.Ok(next.Number);
        }

        private static bool MultiValueVerification(ref PlayerLexer lexer, Tokens valueType)
        {
            if (lexer.NextToken().Type != Tokens.Equals)
                return false;

            if (lexer.NextToken().Type != valueType)
                return false;

            return lexer.NextToken().Type == Tokens.LeftParenthesis;
        }


        private static Result<long> ParseNumberField(ref PlayerLexer lexer)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Equals)
                return Result<long>.Error();
            if ((next = lexer.NextToken()).Type != Tokens.Number)
                return Result<long>.Error();
            return Result<long>.Ok(next.Number);
        }

        private static Result<string> ParseStringField(ref PlayerLexer lexer)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Equals)
                return Result<string>.Error();
            if ((next = lexer.NextToken()).Type != Tokens.String)
                return Result<string>.Error();
            return Result<string>.Ok(next.StringText);
        }

        private static Result<bool> ParseBoolField(ref PlayerLexer lexer)
        {
            Token next;
            if (lexer.NextToken().Type != Tokens.Equals)
                return Result<bool>.Error();
            if ((next = lexer.NextToken()).Type == Tokens.True)
                return Result<bool>.Ok(true);
            if (next.Type == Tokens.False)
                return Result<bool>.Ok(false);
            return Result<bool>.Error();
        }

        private static Player ReportError(string fileName, Token token, Token lastToken)
        {
            Console.WriteLine($"[{fileName}] Bad token {token.Type} after {lastToken.Type}");
            return null;
        }
    }
}