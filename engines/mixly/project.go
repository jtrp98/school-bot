package main

import (
	"encoding/xml"
	"fmt"
	"io"
	"regexp"
	"slices"
	"strconv"
	"strings"
)

type xmlElement struct {
	XMLName  xml.Name
	Attrs    []xml.Attr    `xml:",any,attr"`
	Text     string        `xml:",chardata"`
	Children []*xmlElement `xml:",any"`
}

func (element *xmlElement) attr(name string) string {
	for _, attribute := range element.Attrs {
		if attribute.Name.Local == name {
			return attribute.Value
		}
	}
	return ""
}

func parseProject(source string) ([]*Block, error) {
	// Reject directives and deep trees before Unmarshal allocates the tree.
	decoder := xml.NewDecoder(strings.NewReader(source))
	depth, count := 0, 0
	roots := 0
	for {
		token, err := decoder.Token()
		if err == io.EOF {
			break
		}
		if err != nil {
			return nil, err
		}
		switch element := token.(type) {
		case xml.Directive:
			return nil, fmt.Errorf("XML directives are not supported")
		case xml.ProcInst:
			if element.Target != "xml" || count != 0 {
				return nil, fmt.Errorf("XML processing instructions are not supported")
			}
		case xml.StartElement:
			if depth == 0 {
				roots++
				if roots > 1 {
					return nil, fmt.Errorf("expected one XML root")
				}
			}
			depth++
			count++
			if depth > 96 || count > 4096 {
				return nil, fmt.Errorf("project XML exceeds nesting or element limits")
			}
		case xml.EndElement:
			depth--
		}
	}
	var root xmlElement
	if err := xml.Unmarshal([]byte(source), &root); err != nil {
		return nil, err
	}
	if root.XMLName.Local != "xml" {
		return nil, fmt.Errorf("expected a Blockly xml root")
	}
	blocks := []*Block{}
	for _, element := range root.Children {
		switch element.XMLName.Local {
		case "block":
			block, err := blockFromXML(element)
			if err != nil {
				return nil, err
			}
			blocks = append(blocks, block)
		case "variables":
			// Upstream generators use variable names from block fields, not UI IDs.
			continue
		default:
			return nil, fmt.Errorf("unsupported project element %s", element.XMLName.Local)
		}
	}
	return blocks, nil
}

func blockFromXML(element *xmlElement) (*Block, error) {
	block := &Block{
		Type: element.attr("type"), Fields: map[string]string{}, Values: map[string]*Block{},
		Statements: map[string]*Block{}, Mutation: map[string]string{}, Disabled: element.attr("disabled") == "true",
	}
	for _, child := range element.Children {
		name := child.attr("name")
		switch child.XMLName.Local {
		case "field":
			block.Fields[name] = child.Text
		case "comment":
			block.Comment = child.Text
		case "mutation":
			for _, attribute := range child.Attrs {
				block.Mutation[attribute.Name.Local] = attribute.Value
			}
			for _, argument := range child.Children {
				if argument.XMLName.Local != "arg" || len(argument.Children) != 0 {
					return nil, fmt.Errorf("unsupported nested mutation metadata")
				}
				block.Arguments = append(block.Arguments, Argument{argument.attr("name"), argument.attr("type")})
			}
		case "value", "statement", "next":
			var selected *xmlElement
			for _, candidate := range child.Children {
				if candidate.XMLName.Local != "block" && candidate.XMLName.Local != "shadow" {
					return nil, fmt.Errorf("unsupported connection element")
				}
				if selected == nil || candidate.XMLName.Local == "block" {
					selected = candidate
				}
			}
			if selected == nil {
				return nil, fmt.Errorf("empty block connection")
			}
			nested, err := blockFromXML(selected)
			if err != nil {
				return nil, err
			}
			switch child.XMLName.Local {
			case "value":
				block.Values[name] = nested
			case "statement":
				block.Statements[name] = nested
			case "next":
				block.Next = nested
			}
		default:
			return nil, fmt.Errorf("unsupported block element %s", child.XMLName.Local)
		}
	}
	if name := block.Mutation["name"]; name != "" && block.Fields["NAME"] == "" {
		block.Fields["NAME"] = name
	}
	return block, nil
}

func hasControlCharacter(text string) bool {
	return strings.ContainsFunc(text, func(r rune) bool { return r < 0x20 || r == 0x7f })
}

// Upstream generators paste field text into the C++ unescaped, and comments become `//` lines.
// A line break there would start a new line of C++, such as an #include of a local file whose
// text the compiler then echoes back in its errors; a comment ending in a backslash would join
// the next generated line to the comment and silently drop it.
func checkBlockText(block *Block) error {
	texts := []string{block.Comment}
	for name, value := range block.Fields {
		texts = append(texts, name, value)
	}
	for name, value := range block.Mutation {
		texts = append(texts, name, value)
	}
	for _, argument := range block.Arguments {
		texts = append(texts, argument.Name, argument.Type)
	}
	for _, text := range texts {
		if hasControlCharacter(text) {
			return fmt.Errorf("block %s has a control character such as a line break in its text", block.Type)
		}
	}
	if strings.HasSuffix(strings.TrimRight(block.Comment, " "), `\`) {
		return fmt.Errorf("block %s has a comment ending in a backslash", block.Type)
	}
	return nil
}

type fieldRule func(string) bool

func oneOf(values ...string) fieldRule {
	return func(value string) bool { return slices.Contains(values, value) }
}

var (
	numberLiteral = regexp.MustCompile(`^-?(\d+\.?\d*|\.\d+)([eE][-+]?\d+)?$`).MatchString
	variableType  = oneOf("int", "long", "float", "boolean", "String")
	returnType    = oneOf("", "void", "int", "long", "float", "boolean", "String")
	argumentType  = oneOf("", "int", "long", "float", "boolean", "String")
	serialPort    = oneOf("Serial")
	// Blockly's safeName_ turns every character outside \w into _ except the comma.
	nameRule fieldRule = func(name string) bool { return !strings.Contains(name, ",") }
)

// arduinoPastedFields lists, per block type, the fields whose text the upstream Arduino
// generators place in the C++ as it is, with the values each may take: anything else would be
// program code on that line. School Bot's sb_* generators check their own fields.
var arduinoPastedFields = map[string]map[string]fieldRule{
	"math_number":             {"NUM": numberLiteral},
	"controls_delay":          {"UNIT": oneOf("delay", "delayMicroseconds")},
	"math_to_int":             {"OP": oneOf("round", "ceil", "floor", "abs", "sqrt")},
	"controls_for":            {"VAR": nameRule},
	"variables_declare":       {"VAR": nameRule, "TYPE": variableType},
	"variables_get":           {"VAR": nameRule},
	"variables_set":           {"VAR": nameRule},
	"sb_variable_change":      {"VAR": nameRule},
	"procedures_defnoreturn":  {"NAME": nameRule, "TYPE": returnType},
	"procedures_defreturn":    {"NAME": nameRule, "TYPE": returnType},
	"procedures_callnoreturn": {"NAME": nameRule},
	"procedures_callreturn":   {"NAME": nameRule},
	"serial_print":            {"serial_select": serialPort, "new_line": oneOf("print", "println")},
	"serial_readstr":          {"serial_select": serialPort},
	"serial_available":        {"serial_select": serialPort},
}

func checkPastedFields(block *Block) error {
	for name, accepts := range arduinoPastedFields[block.Type] {
		if !accepts(block.Fields[name]) {
			return fmt.Errorf("block %s field %s holds a value the block does not offer", block.Type, name)
		}
	}
	for _, argument := range block.Arguments {
		if !nameRule(argument.Name) || !argumentType(argument.Type) {
			return fmt.Errorf("block %s has an argument name or type the block does not offer", block.Type)
		}
	}
	return nil
}

func validateBlocks(blocks []*Block, language string) error {
	count := 0
	var visit func(*Block, int) error
	visit = func(block *Block, depth int) error {
		count++
		if block == nil || block.Type == "" || len(block.Type) > 128 {
			return fmt.Errorf("every block needs a type")
		}
		if count > 512 || depth > 32 {
			return fmt.Errorf("use at most 512 blocks and 32 nested connections")
		}
		for _, children := range []map[string]*Block{block.Values, block.Statements} {
			for _, child := range children {
				if err := visit(child, depth+1); err != nil {
					return err
				}
			}
		}
		if err := checkBlockText(block); err != nil {
			return err
		}
		if language == "arduino" {
			if err := checkPastedFields(block); err != nil {
				return err
			}
		}
		for name, value := range block.Mutation {
			if len(name) > 64 || len(value) > 128 {
				return fmt.Errorf("mutation metadata is too large")
			}
			switch name {
			case "items", "elseif", "else":
				amount, err := strconv.Atoi(value)
				if err != nil || amount < 0 || amount > 512 {
					return fmt.Errorf("invalid mutation count %s", name)
				}
			}
		}
		if len(block.Arguments) > 64 {
			return fmt.Errorf("use at most 64 procedure arguments")
		}
		// The next block sits in the same row, not inside this one; the block count bounds a row.
		if block.Next != nil {
			return visit(block.Next, depth)
		}
		return nil
	}
	if len(blocks) == 0 {
		return fmt.Errorf("project has no blocks")
	}
	for _, block := range blocks {
		if err := visit(block, 1); err != nil {
			return err
		}
	}
	return nil
}
