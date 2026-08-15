-- =========================================================
-- BASE DE DATOS: chatbot_db
-- =========================================================

CREATE DATABASE IF NOT EXISTS chatbot_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE chatbot_db;


-- =========================================================
-- TABLA: usuarios
-- =========================================================

CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255),
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
);


-- =========================================================
-- TABLA: conversaciones
-- Una conversación pertenece a un usuario.
-- =========================================================

CREATE TABLE conversaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    titulo VARCHAR(255),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_conversacion_usuario
        FOREIGN KEY (usuario_id)
        REFERENCES usuarios(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_conversaciones_usuario (usuario_id),
    INDEX idx_conversaciones_fecha (fecha_actualizacion)
);


-- =========================================================
-- TABLA: mensajes
-- Almacena todos los mensajes de una conversación.
-- =========================================================

CREATE TABLE mensajes (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    conversacion_id INT NOT NULL,

    tipo_emisor ENUM('USUARIO', 'SISTEMA') NOT NULL,

    contenido TEXT NOT NULL,

    fecha_envio DATETIME DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_mensaje_conversacion
        FOREIGN KEY (conversacion_id)
        REFERENCES conversaciones(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_mensajes_conversacion (conversacion_id),
    INDEX idx_mensajes_fecha (fecha_envio)
);


-- =========================================================
-- TABLA: mensajes_adjuntos
-- Guarda imágenes o archivos asociados a un mensaje y conversación.
-- =========================================================

CREATE TABLE mensajes_adjuntos (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    mensaje_id BIGINT NOT NULL,
    conversacion_id INT NOT NULL,
    tipo_archivo VARCHAR(50) NOT NULL DEFAULT 'IMAGEN',
    mime_type VARCHAR(100),
    nombre_archivo VARCHAR(255),
    base64 LONGTEXT,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_adjunto_mensaje
        FOREIGN KEY (mensaje_id)
        REFERENCES mensajes(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT fk_adjunto_conversacion
        FOREIGN KEY (conversacion_id)
        REFERENCES conversaciones(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_adjuntos_mensaje (mensaje_id),
    INDEX idx_adjuntos_conversacion (conversacion_id)
);