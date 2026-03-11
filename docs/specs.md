# PAW-Ntritional-Planner — Specificații și descriere funcționalități

Data: 2026-03-11

Acest document descrie pe larg funcționalitățile aplicației "PAW-Ntritional-Planner", ce este implementat în prezent în repo și ce va fi implementat în iterațiile viitoare. Scopul este să ofere o referință clară pentru dezvoltare, testare și demonstrație.

## 1. Rezumat proiect

PAW-Ntritional-Planner este o aplicație web statică (frontend-first) pentru planificare nutrițională și urmărirea meselor, cu pagini HTML simple și componente UI construite cu Bootstrap 5. Designul actual urmărește un prototip funcțional, ușor de extins cu JavaScript pentru interactivitate și/sau cu un backend pentru persistare.

## 2. Domeniu și obiective

- Scop imediat: interfață UI (wireframe) pentru paginile principale: home, dashboard, profile, login/register, shopping-list, meals (meal journal), planner, recipes.
- Scop pe termen mediu: interactivitate client-side (adăugare elemente în shopping list, filtrare rețete, adăugare în jurnal), validări, persistare locală (localStorage) și/sau backend minim (API REST) pentru salvare conturi și date.

## 3. Ce este implementat (stare curentă UI)

Observație: enumerarea de mai jos reflectă structura și componentele UI care au fost adăugate în fișierele HTML din repo.

- Pagini HTML (root): `index.html`, `dashboard.html`, `profile.html`, `login.html`, `register.html`, `meals.html`, `planner.html`, `recipes.html`, `shopping-list.html`.
- Integrare stil: Bootstrap 5 (CDN) + `assets/css/style.css` pentru mici ajustări.
- Navbar & footer: design comun (s-a folosit o soluție de partials client-side în iterațiile anterioare; în repo pot exista variante statice). Navbar include linkuri către paginile principale și butoane Login/Register.
- Dashboard: card "CALORIES" cu atribute `data-consumed` și `data-target` și un progress bar pentru afișarea procentului din țintă.
- Profile: card cont (avatar placeholder, nume, email) și card "Nutritional goals" cu input numeric pentru "Daily calorie target" și select pentru Goal (Lose/Maintain/Gain).
- Shopping list: card cu câmp de intrare + buton "Add" și o listă mock de elemente cu checkbox (UI only).
- Planner: tabel săptămânal responsive cu rânduri pentru Breakfast/Lunch/Dinner/Snacks și coloane pentru zilele săptămânii (UI only).
- Recipes: bară de căutare și listă mock de rețete cu valori calorice vizibile (UI only).
- Meals (Meal journal): tabel mock de înregistrări ale meselor și UI pentru adăugare rânduri (UI only).

## 4. Funcționalități cerute (detaliat) — prezente și planificate

Pentru fiecare funcționalitate se dă: descriere, intrări/ieșiri, comportamente de eroare, criterii de acceptare.

4.1 Autentificare (Login / Register)
- Descriere: pagini statice pentru autentificare — fără backend în etapa inițială.
- Intrări: email, parolă, confirmare parolă la înregistrare.
- Ieșiri: în etapa inițială, doar validări client-side; ulterior, token/session de server.
- Erori: validări câmpuri goale, format email invalid, neconcordanță parole.
- Acceptare: formulare validate local; trimiterea (simulată) afișează un mesaj de succes.

4.2 Profile & Nutritional Goals
- Descriere: afișare datelor de utilizator și setare target caloric zilnic + obiectiv (Lose/Maintain/Gain).
- Intrări: numeric (calorii), select opțiune.
- Ieșiri: actualizare UI; stocare locală (opțional) sau trimitere la API.
- Acceptare: date salvate localStorage sau confirmare vizuală.

4.3 Shopping list
- Descriere: adăugare elemente, marcare ca cumpărat, eliminare elemente.
- Intrări: text produs, buton Add, checkbox pentru marcat.
- Ieșiri: elemente afișate în listă; posibil persist în localStorage.
- Acceptare: elementul apare în listă la click Add; checkbox funcțional; persistare locală (dacă implementată).

4.4 Planner săptămânal
- Descriere: tabel pentru atribuirea meselor pe zile; interacțiune de edit-in-place (planificare rapidă).
- Intrări: selecție/edite text în celule, buton Save.
- Ieșiri: tabel actualizat; posibil export/print.
- Acceptare: modificările reflectate în UI; persistare locală sau la server.

4.5 Meal journal (Jurnal mese)
- Descriere: înregistrare mâncărurilor consumate (dată, masă, porție, calorii estimate).
- Intrări: form add meal (name, meal type, calories, notes).
- Ieșiri: rând nou în jurnal; agregare calorii zilnice (folosită în Dashboard).
- Acceptare: rândul apare în tabel; totalul caloric e recalculat.

4.6 Recipes
- Descriere: căutare și listare rețete cu informații sumare (calorii). Posibilă salvare ca favorite.
- Intrări: query text, click Favorite/Save.
- Ieșiri: listă filtrată; stare favorite per utilizator.
- Acceptare: căutarea filtrează elementele din listă; favorite se marchează vizual.

4.7 Dashboard — Overview
- Descriere: afișare progres calorii pentru ziua curentă, cardio/greutate/etc (extensibil).
- Intrări: date din meal journal sau target din profile.
- Ieșiri: progress bar actualizat, carduri rezumative.
- Acceptare: progress reflectă datele jurnalului; procent corect calculat.

## 5. Modele de date (propuse)

- User
	- id: string (UUID)
	- name: string
	- email: string
	- passwordHash: string (dacă este stocat server-side)
	- dailyTarget: integer
	- goal: enum [lose, maintain, gain]

- MealEntry
	- id: string
	- userId: string
	- date: ISODate
	- mealType: enum [breakfast, lunch, dinner, snack]
	- name: string
	- calories: integer
	- notes: string

- ShoppingItem
	- id: string
	- userId: string
	- name: string
	- purchased: boolean

- Recipe
	- id: string
	- title: string
	- calories: integer
	- ingredients: [string]
	- instructions: string

Observație: la început se poate folosi `localStorage` pentru persistare per utilizator (în absența unui backend).

## 6. API (propuneri pentru etapă ulterioară)

- POST /api/register {name,email,password}
- POST /api/login {email,password} -> {token}
- GET /api/me -> {user}
- GET /api/me/meals?date=YYYY-MM-DD -> [MealEntry]
- POST /api/me/meals {meal entry}
- PUT/DELETE pentru meal/shopping items

Autentificare: JWT în header Authorization: Bearer <token>

## 7. Cerințe non-funcționale

- Răspuns UI: încărcare rapidă, Bootstrap responsive (mobile-first).
- Compatibilitate: browsere moderne (Chrome, Edge, Firefox).
- Securitate (când apare backend): parole criptate (bcrypt), comunicație TLS, validări server-side.
- Persistență: inițial localStorage; ulterior DB relațional sau NoSQL (ex: PostgreSQL/MongoDB).

## 8. Criterii de acceptare & testare

- Fiecare pagină se încarcă fără erori JS în consola browser.
- Formulare validează și afișează mesaje de eroare pentru intrări invalide.
- Adăugarea elementelor în shopping list și jurnalul de mese funcționează (UI) și, dacă se activează, persistă în localStorage.
- Dashboard afișează corect procentul bazat pe datele din jurnal și ținta setată.

Testare recomandată:
- Unit & integration tests pentru orice logică JS adăugată (jest / vitest).
- Smoke test manual pentru layout pe desktop și mobil.

## 9. Limitări și asumptii

- Documentul pornește de la premisele unei aplicații statice cu potențial de extindere.
- Include design UI-only pentru multe componente; funcționalitatea completă depinde de implementarea JS/back-end ulterioară.

## 10. Pași următori (implantare imediată)

1. Stabilire priorități: 1) Shopping list interactivă, 2) Meal journal (adăugare + agregare calorii), 3) Recipes search client-side, 4) Persistență localStorage.
2. Implementare JS minimal pentru funcționalități de mai sus + teste unitare.
3. Dacă se dorește multi-user, proiectare API și migrare la server (express/fastify + PostgreSQL sau Firebase pentru prototip rapid).

## 11. Anexe

- Fișiere relevante:
	- `index.html` — pagină home/hero
	- `dashboard.html` — overview calorii
	- `profile.html` — cont & goals
	- `login.html`, `register.html` — formulare autentificare
	- `meals.html`, `planner.html`, `recipes.html`, `shopping-list.html` — pagini de funcționalitate
	- `assets/css/style.css` — stiluri adiționale

Pentru detalii suplimentare, pot adapta acest document la format cerut (pdf / prezentare) sau pot transforma fiecare secțiune într-un backlog de issue-uri (GitHub issues) cu estimări și priorități.
