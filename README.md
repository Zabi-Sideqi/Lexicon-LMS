````markdown
# Lexicon LMS

Lexicon LMS slutprojekt 2026.

## GitHub Workflow

Vi arbetar tillsammans genom GitHub och använder samma arbetsflöde för alla tasks.

### 1. Ta en task

Alla tasks finns i vårt GitHub Project.

När en task är redo att börja arbetar vi enligt:

**Backlog → Ready → In Progress**

När du tar en task flyttar du den till **In Progress**.

Varje utvecklare arbetar på sin egen feature-branch.

### 2. Skapa en branch

Vi arbetar inte direkt på `main` eller `development`.

När du tar en task:

1. Börja från `development`.
2. Skapa en egen feature-branch.
3. Arbeta med din task på den branchen.

Exempel:

```text
development
    ↓
feature/user-controller
````

Försök att använda ett enkelt branch-namn som visar vad du arbetar med.

### 3. När du är klar med din task

När implementationen är klar:

1. Testa att koden fungerar lokalt.
2. Kontrollera att projektet bygger utan fel.
3. Kör relevanta tester.
4. Pusha din branch till GitHub.
5. Skapa en Pull Request mot `development`.
6. Flytta tasken till **In review**.

### 4. Code Review och Test

När en Pull Request är skapad ska en annan person i gruppen granska koden.

Vi ska både läsa koden och testa funktionen.

Under review kontrollerar vi till exempel:

* Fungerar funktionen enligt User Story och Acceptance Criteria?
* Fungerar validation?
* Fungerar authorization?
* Finns det buggar eller fel?
* Är koden lätt att förstå?
* Finns relevanta tester?
* Bygger projektet utan fel?

Om något behöver ändras skriver vi en kommentar på Pull Requesten.

Utvecklaren gör ändringarna på sin egen branch och pushar igen.

Sedan testar och granskar vi igen.

### 5. När Pull Request är godkänd

När:

* koden fungerar
* testerna fungerar
* review är klar
* alla review-kommentarer är hanterade

kan Pull Requesten godkännas.

Därefter mergas den till:

```text
development
```

När tasken är färdig flyttas den till:

**Done**

### 6. Från development till main

Vi mergar inte direkt från en feature-branch till `main`.

När en större del av arbetet är färdig och `development` är testad skapar vi en Pull Request:

```text
development → main
```

Vi testar och kontrollerar att allt fungerar innan Pull Requesten mergas.

`main` ska innehålla en fungerande version av projektet.

### 7. Azure och CI/CD

Vår `main`-branch är kopplad till Azure genom CI/CD.

När ändringar mergas till `main` startar CI/CD-flödet och projektet deployas till Azure enligt vår konfiguration.

Vi behöver därför inte göra deployment manuellt varje gång.

Vårt arbetsflöde ser ut så här:

```text
Task
  ↓
Feature branch
  ↓
Pull Request
  ↓
Code Review + Test
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

### 8. Viktiga regler

* Ingen pushar direkt till `main`.
* Ingen arbetar direkt på `main`.
* Alla arbetar på sin egen feature-branch.
* Feature-branch skapas från `development`.
* Alla ändringar går genom Pull Request.
* En annan teammedlem ska göra code review.
* Vi testar innan vi mergar.
* Review-kommentarer ska hanteras innan merge.
* `development` används för gemensam utveckling.
* `main` ska innehålla en fungerande version.
* Azure deployment sker genom CI/CD från `main`.

## Definition of Done

En task är klar när:

* implementationen är färdig
* Acceptance Criteria är uppfyllda
* projektet bygger och körs utan fel
* nödvändig validation och authorization finns
* relevanta tester är genomförda
* Pull Request är skapad
* code review är genomförd
* review-kommentarer är hanterade
* Pull Request är mergad till `development`
* task/sub-issue är klar
* README uppdateras om dokumentationen påverkas

```


