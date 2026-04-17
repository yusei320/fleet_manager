-- Script de création de la base de données SQLite pour Fleet Manager
-- Exécuter ce script pour créer la structure de base de données initiale

-- Création de la table utilisateurs
CREATE TABLE IF NOT EXISTS utilisateurs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nom TEXT NOT NULL,
    prenom TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    mot_de_passe TEXT NOT NULL,
    role TEXT NOT NULL CHECK (role IN ('Administrateur', 'Utilisateur')),
    bloque_jusqu DATETIME NULL,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Création de la table vehicules
CREATE TABLE IF NOT EXISTS vehicules (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    immatriculation TEXT UNIQUE NOT NULL,
    marque TEXT NOT NULL,
    modele TEXT NOT NULL,
    annee INTEGER,
    carburant TEXT,
    kilometrage INTEGER DEFAULT 0,
    id_utilisateur INTEGER NOT NULL,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_utilisateur) REFERENCES utilisateurs(id) ON DELETE CASCADE
);

-- Création de la table suivi
CREATE TABLE IF NOT EXISTS suivi (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    id_vehicule INTEGER NOT NULL,
    date_suivi DATETIME NOT NULL,
    carburant_litre REAL,
    cout REAL,
    distance_km REAL,
    commentaire TEXT,
    date_creation DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_vehicule) REFERENCES vehicules(id) ON DELETE CASCADE
);

-- Insertion de l'utilisateur administrateur par défaut
-- Le mot de passe 'admin123' est haché avec BCrypt
DELETE FROM utilisateurs;
INSERT INTO utilisateurs (id, nom, prenom, email, mot_de_passe, role) 
VALUES (1, 'Admin', 'Super', 'admin@fleetmanager.com', '$2a$12$5fOE2WYChxrj0p3vqhvss.5cBrLiVFxoz5JoPRE1bkzEP1Q99fW1O', 'Administrateur');

-- Insertion d'utilisateurs fictifs pour les tests
-- Mot de passe : user123 (haché avec BCrypt)
INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Dupont', 'Jean', 'jean.dupont@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Martin', 'Marie', 'marie.martin@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Bernard', 'Pierre', 'pierre.bernard@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Petit', 'Sophie', 'sophie.petit@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Robert', 'Luc', 'luc.robert@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Richard', 'Claire', 'claire.richard@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Administrateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Durand', 'Michel', 'michel.durand@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Lefebvre', 'Isabelle', 'isabelle.lefebvre@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Moreau', 'Thomas', 'thomas.moreau@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

INSERT INTO utilisateurs (nom, prenom, email, mot_de_passe, role) 
VALUES ('Simon', 'Emma', 'emma.simon@example.com', '$2a$12$8K1p/a0dL5xX7yZ9qB2n1OeX3vY4zA5bC6dE7fG8hI9jK0lM1nO2p', 'Utilisateur');

-- Création d'index pour optimiser les performances
CREATE INDEX IF NOT EXISTS idx_vehicules_utilisateur ON vehicules(id_utilisateur);
CREATE INDEX IF NOT EXISTS idx_suivi_vehicule ON suivi(id_vehicule);
CREATE INDEX IF NOT EXISTS idx_suivi_date ON suivi(date_suivi);
CREATE INDEX IF NOT EXISTS idx_utilisateurs_email ON utilisateurs(email);
