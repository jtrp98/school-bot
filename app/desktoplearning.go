package main

import (
	"bytes"
	"context"
	"crypto/sha256"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"strconv"
	"time"
)

type learningRequest struct {
	Operation   string          `json:"operation"`
	ID          int             `json:"id"`
	CourseRunID int             `json:"course_run_id"`
	ActivityID  int             `json:"activity_id"`
	Body        json.RawMessage `json:"body"`
	Key         string          `json:"key"`
	ETag        string          `json:"etag"`
}

func (s *DesktopService) LearningScope() (string, error) {
	s.mu.Lock()
	defer s.mu.Unlock()
	if s.session == nil {
		return "", errors.New("AUTH_REQUIRED")
	}
	school, user, expiry, err := gatewayClaims(s.session.Token)
	if err != nil || time.Now().Unix() >= expiry {
		return "", errors.New("AUTH_REQUIRED")
	}
	digest := sha256.Sum256([]byte(s.apiURL + "|" + school + "|" + user))
	return fmt.Sprintf("%x", digest[:]), nil
}

func learningRoute(r learningRequest) (string, string, error) {
	root := "/api/service-school-bot"
	id := strconv.Itoa(r.ID)
	switch r.Operation {
	case "courseRuns":
		if r.ID > 0 {
			return "GET", root + "/learning/curricula/" + id + "/course-runs", nil
		}
	case "curricula":
		return "GET", root + "/curricula", nil
	case "curriculum":
		if r.ID > 0 {
			return "GET", root + "/learning/curricula/" + id, nil
		}
	case "project":
		return "POST", root + "/projects", nil
	case "submission":
		return "POST", root + "/submissions", nil
	case "submissionResult":
		if r.ID > 0 {
			return "GET", root + "/submissions/" + id, nil
		}
	case "submissions":
		return "GET", root + "/submissions", nil
	case "review":
		if r.ID > 0 {
			return "POST", root + "/submissions/" + id + "/assessments", nil
		}
	case "progress":
		if r.CourseRunID > 0 {
			return "GET", root + "/progress?course_run_id=" + strconv.Itoa(r.CourseRunID), nil
		}
	case "startAttempt":
		if r.CourseRunID > 0 && r.ActivityID > 0 {
			return "POST", root + "/course-runs/" + strconv.Itoa(r.CourseRunID) + "/activities/" + strconv.Itoa(r.ActivityID) + "/attempts", nil
		}
	case "attempt":
		if r.ID > 0 {
			return "GET", root + "/attempts/" + id, nil
		}
	case "answers":
		if r.ID > 0 {
			return "PUT", root + "/attempts/" + id + "/answers", nil
		}
	case "submitAttempt":
		if r.ID > 0 {
			return "POST", root + "/attempts/" + id + "/submit", nil
		}
	}
	return "", "", errors.New("unsupported learning operation")
}

func (s *DesktopService) LearningRequest(raw string) (string, error) {
	if len(raw) > 1024*1024 {
		return "", errors.New("learning request too large")
	}
	var input learningRequest
	decoder := json.NewDecoder(bytes.NewBufferString(raw))
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(&input); err != nil {
		return "", err
	}
	method, path, err := learningRoute(input)
	if err != nil {
		return "", err
	}
	if err = validateAPIURL(s.apiURL); err != nil {
		return "", err
	}
	s.mu.Lock()
	token := ""
	if s.session != nil {
		token = s.session.Token
	}
	s.mu.Unlock()
	if token == "" || s.entitlement == nil {
		return "", errors.New("AUTH_REQUIRED")
	}
	ctx, cancel := context.WithTimeout(context.Background(), 15*time.Second)
	defer cancel()
	entitlement, err := s.entitlement.DeviceEntitlement(ctx)
	if err != nil {
		return "", err
	}
	body := input.Body
	if len(body) == 0 {
		body = json.RawMessage(`{}`)
	}
	request, err := http.NewRequestWithContext(ctx, method, s.apiURL+path, bytes.NewReader(body))
	if err != nil {
		return "", err
	}
	request.Header.Set("Authorization", "Bearer "+token)
	request.Header.Set("X-Device-Entitlement", entitlement)
	request.Header.Set("Content-Type", "application/json")
	if input.Key != "" {
		request.Header.Set("Idempotency-Key", input.Key)
	}
	if input.ETag != "" {
		request.Header.Set("If-Match", input.ETag)
	}
	client := &http.Client{CheckRedirect: func(*http.Request, []*http.Request) error { return errors.New("redirect refused") }}
	response, err := client.Do(request)
	if err != nil {
		return "", fmt.Errorf("LEARNING_NETWORK: %w", err)
	}
	defer response.Body.Close()
	payload, err := io.ReadAll(io.LimitReader(response.Body, 2*1024*1024+1))
	if err != nil || len(payload) > 2*1024*1024 || !json.Valid(payload) {
		return "", errors.New("invalid learning response")
	}
	if response.StatusCode < 200 || response.StatusCode >= 300 {
		return "", fmt.Errorf("LEARNING_API_%d: %s", response.StatusCode, payload)
	}
	result, err := json.Marshal(struct {
		Data json.RawMessage `json:"data"`
		ETag string          `json:"etag"`
	}{payload, response.Header.Get("ETag")})
	return string(result), err
}
