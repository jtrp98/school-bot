package main

import "testing"

func TestLearningRoutesAreFixedAndRejectUnknownOperations(t *testing.T) {
	cases := []struct {
		operation, method, path string
		id, run, activity       int
	}{
		{"curricula", "GET", "/api/service-school-bot/curricula", 0, 0, 0},
		{"curriculum", "GET", "/api/service-school-bot/learning/curricula/7", 7, 0, 0},
		{"submission", "POST", "/api/service-school-bot/submissions", 0, 0, 0},
		{"startAttempt", "POST", "/api/service-school-bot/course-runs/3/activities/4/attempts", 0, 3, 4},
		{"submitAttempt", "POST", "/api/service-school-bot/attempts/9/submit", 9, 0, 0},
	}
	for _, row := range cases {
		method, path, err := learningRoute(learningRequest{Operation: row.operation, ID: row.id, CourseRunID: row.run, ActivityID: row.activity})
		if err != nil || method != row.method || path != row.path {
			t.Fatalf("%s: %s %s %v", row.operation, method, path, err)
		}
	}
	for _, operation := range []string{"https://evil.example", "../projects", "attempt"} {
		if _, _, err := learningRoute(learningRequest{Operation: operation}); err == nil {
			t.Fatal("invalid operation accepted")
		}
	}
}
