# 🎓 Lexicon LMS

Lexicon LMS är vårt slutprojekt på Lexicon 2026.

Projektet utvecklas tillsammans i GitHub och vi använder GitHub Projects för att planera och följa vårt arbete.

---

## 📋 Projektstruktur

Vi arbetar med **User Stories** och **Development Tasks**.

* **User Story** = Vad användaren ska kunna göra
* **Development Task** = Vad vi behöver utveckla för att uppfylla User Storyn
* **Sprint** = Vilken sprint arbetet tillhör
* **Pull Request** = Används för code review innan merge

---

## 🔄 GitHub Workflow

Vi använder följande arbetsflöde:

```text
Backlog
   ↓
Ready
   ↓
In Progress
   ↓
In Review
   ↓
Done
```

---

## 1. 📝 Ta en task

Alla våra User Stories och Development Tasks finns i vårt **GitHub Project**.

När du vill börja arbeta med en task:

1. Välj en task från **Ready**.
2. Flytta tasken till **In Progress**.
3. Kontrollera vad som behöver göras och vilka Acceptance Criteria som finns.
4. Skapa en egen feature-branch.

---

## 2. 🌿 Skapa en feature-branch

Vi arbetar inte direkt på `main` eller `development`.

Alla utvecklare arbetar på sin egen branch.

En ny branch ska skapas från:

```text
development
```

Exempel:

```text
development
      ↓
feature/user-controller
```

Branch-namnet ska helst beskriva vad du arbetar med.

Exempel:

```text
feature/course-controller
feature/login
feature/student-dashboard
feature/document-upload
```

---

## 3. 💻 Arbeta med tasken

När branchen är skapad arbetar du med din task.

Under utvecklingen ska du:

* Följa User Story och Acceptance Criteria
* Skriva nödvändig kod
* Göra validation där det behövs
* Kontrollera authorization där det behövs
* Skriva relevanta tester
* Testa funktionen lokalt
* Kontrollera att projektet bygger utan fel

När du arbetar med tasken ligger den i:

**In Progress**

---

## 4. 🚀 Skapa Pull Request

När du är klar med implementationen:

1. Testa koden lokalt.
2. Kontrollera att projektet bygger.
3. Kör relevanta tester.
4. Pusha din branch till GitHub.
5. Skapa en **Pull Request** mot `development`.
6. Flytta tasken till **In Review**.

Exempel:

```text
feature/user-controller
          ↓
     Pull Request
          ↓
     development
```

---

## 👀 Code Review

En annan teammedlem ska granska Pull Requesten.

Under review kontrollerar vi bland annat:

* Fungerar implementationen?
* Är Acceptance Criteria uppfyllda?
* Fungerar validation?
* Fungerar authorization?
* Finns det buggar?
* Är koden lätt att förstå?
* Finns relevanta tester?
* Bygger projektet utan fel?

Om något behöver ändras skriver vi en kommentar i Pull Requesten.

Utvecklaren gör ändringarna på sin branch och pushar igen.

Sedan granskas och testas ändringarna igen.

---

## ✅ När Pull Request är godkänd

Pull Request kan mergas när:

* implementationen fungerar
* testerna fungerar
* code review är klar
* review-kommentarer är hanterade
* Acceptance Criteria är uppfyllda

Pull Requesten mergas då till:

```text
development
```

När tasken är färdig flyttas den till:

**Done**

---

## 🔀 Development → Main

Vi mergar inte direkt från en feature-branch till `main`.

När en större del av arbetet är färdig och `development` är testad skapar vi en Pull Request:

```text
development
      ↓
 Pull Request
      ↓
     main
```

Vi testar och kontrollerar att projektet fungerar innan Pull Requesten mergas.

`main` ska innehålla en fungerande version av projektet.

---

## ☁️ Azure & CI/CD

Vår `main`-branch är kopplad till Azure genom CI/CD.

När ändringar mergas till `main` startar vårt CI/CD-flöde och projektet deployas till Azure enligt vår konfiguration.

Det betyder att deployment kan ske automatiskt istället för att vi behöver göra det manuellt varje gång.

```text
Feature Branch
      ↓
Pull Request
      ↓
Code Review
      ↓
development
      ↓
Test
      ↓
Pull Request
      ↓
main
      ↓
CI/CD
      ↓
Azure
```

---

## 📌 Viktiga regler

> **Vi använder Pull Requests för alla ändringar.**

* ❌ Ingen push direkt till `main`
* ❌ Ingen arbetar direkt på `main`
* ❌ Ingen arbetar direkt på `development`
* ✅ Alla arbetar på sin egen feature-branch
* ✅ Feature-branch skapas från `development`
* ✅ Pull Request används för merge
* ✅ En annan teammedlem gör code review
* ✅ Vi testar innan merge
* ✅ Review-kommentarer ska hanteras
* ✅ `development` används för gemensam utveckling
* ✅ `main` ska innehålla en fungerande version

---

## 🏁 Definition of Done

En task är **Done** när:

* [ ] Implementation är färdig
* [ ] Acceptance Criteria är uppfyllda
* [ ] Projektet bygger utan fel
* [ ] Projektet fungerar lokalt
* [ ] Nödvändig validation finns
* [ ] Nödvändig authorization finns
* [ ] Relevanta tester är genomförda
* [ ] Pull Request är skapad
* [ ] Code review är genomförd
* [ ] Review-kommentarer är hanterade
* [ ] Pull Request är mergad till `development`
* [ ] Task/Sub-issue är klar
* [ ] README uppdateras om dokumentationen påverkas

---

## 🏃 Sprints

Projektet är uppdelat i fyra sprints:

| Sprint       | User Stories |
| ------------ | ------------ |
| **Sprint 1** | US00 – US04  |
| **Sprint 2** | US05 – US08  |
| **Sprint 3** | US09 – US13  |
| **Sprint 4** | US14 – US17  |

---

## 🛠️ Projektverktyg

Vi använder bland annat:

* **GitHub** – kod och Pull Requests
* **GitHub Projects** – planering och Kanban-board
* **GitHub Issues** – User Stories och Development Tasks
* **Azure** – deployment
* **CI/CD** – automatiserad build och deployment
