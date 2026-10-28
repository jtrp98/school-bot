package main

import (
	"encoding/json"
	"io/fs"
	"path"
	"sort"
	"strings"
)

type Board struct {
	ID       string                     `json:"id"`
	Language string                     `json:"language"`
	Index    string                     `json:"-"`
	Targets  map[string]json.RawMessage `json:"targets"`
	Config   map[string]json.RawMessage `json:"config"`
}

func catalog() ([]Board, error) {
	directories, err := fs.ReadDir(upstream, "third_party/mixly/board")
	if err != nil {
		return nil, err
	}
	boards := []Board{}
	for _, directory := range directories {
		configPath := "third_party/mixly/board/" + directory.Name() + "/config.json"
		data, err := upstream.ReadFile(configPath)
		if err != nil {
			continue
		}
		board := Board{ID: directory.Name(), Language: "python"}
		if strings.HasPrefix(board.ID, "arduino_") {
			board.Language = "arduino"
		}
		if err := json.Unmarshal(data, &board.Config); err != nil {
			return nil, err
		}
		if targets := board.Config["board"]; targets != nil {
			if err := json.Unmarshal(targets, &board.Targets); err != nil {
				return nil, err
			}
		}
		pages, _ := fs.Glob(upstream, path.Dir(configPath)+"/index*.html")
		if len(pages) == 0 {
			continue
		}
		board.Index = strings.TrimPrefix(pages[0], "third_party/mixly/")
		boards = append(boards, board)
	}
	sort.Slice(boards, func(i, j int) bool { return boards[i].ID < boards[j].ID })
	return boards, nil
}

func findBoard(id string) (Board, bool) {
	boards, err := catalog()
	if err != nil {
		return Board{}, false
	}
	for _, board := range boards {
		if board.ID == id {
			return board, true
		}
	}
	return Board{}, false
}
