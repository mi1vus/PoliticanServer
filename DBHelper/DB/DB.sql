CREATE SCHEMA IF NOT EXISTS `politician` ;

DROP VIEW IF EXISTS `politician`.`users_rating`;

DROP TABLE IF EXISTS `politician`.`users_history`;

DROP TABLE IF EXISTS `politician`.`users`;

CREATE TABLE IF NOT EXISTS `politician`.`users` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `nickname` VARCHAR(150) NOT NULL,
  `fname` VARCHAR(150) NOT NULL,
  `lname` VARCHAR(150) NOT NULL,
  `midname` VARCHAR(150) NOT NULL,
  `city` VARCHAR(150) NOT NULL,
  `mail` VARCHAR(150) NOT NULL,
  `stage` INT NOT NULL DEFAULT 1,
  `exam`  BOOLEAN NOT NULL DEFAULT false,
  `exam_pass` VARCHAR(32),
  `pass` VARCHAR(32) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `users_name_UNIQUE` (`nickname`  ASC)
);

CREATE TABLE IF NOT EXISTS `politician`.`users_history` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `date` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `stage` INT NOT NULL,
  `score` INT NOT NULL,
  `id_user` INT NOT NULL,
  `msg` TEXT NOT NULL,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`id_user`) REFERENCES `politician`.`users`(`id`)
);

CREATE VIEW `politician`.`users_rating` AS 
select `u`.`id`, `u`.`nickname`, `u`.`fname`, `u`.`lname`, `u`.`midname`, `u`.`city`, `u`.`mail`, `u`.`stage`, `u`.`exam`, sum(`score`) `reward`
from `politician`.`users_history` AS `h`
left Join `politician`.`users` AS `u` ON `u`.`id` = `h`.`id_user`
group by `id_user`;

INSERT INTO `politician`.`users` (`id`, `nickname`, `fname`, `lname`, `midname`, `city`, `mail`, `stage`, `exam`,`exam_pass`,  `pass`) VALUES 
('1', 'AutoAdmin', 'AutoAdmin1', 'AutoAdmin2', 'AutoAdmin3', 'AutoAdminBurg', 'AutoAdmin@game.ru', 1, false, null, '81dc9bdb52d04dc20036dbd8313ed055'),
('2', 'Admin', 'Admin1', 'Admin2', 'Admin3', 'AdminBurg', 'Admin@game.ru', 1, true, '81dc9bdb52d04dc20036dbd8313ed055', '81dc9bdb52d04dc20036dbd8313ed055'),
('3', 'Max', 'Max1', 'Max2', 'Max', 'MaxBurg3', 'Max@game.ru', 2, false, null, '81dc9bdb52d04dc20036dbd8313ed055'),
('4', 'Lisa', 'Lisa1', 'Lisa2', 'Lisa3', 'LisaBurg', 'Lisa@game.ru', 2, true, '81dc9bdb52d04dc20036dbd8313ed055', '81dc9bdb52d04dc20036dbd8313ed055'),
('5', 'John', 'John1', 'John2', 'John3', 'JohnBurg', 'John@game.ru', 3, false, null, '81dc9bdb52d04dc20036dbd8313ed055'),
('6', 'Doug', 'Doug1', 'Doug2', 'Doug3', 'DougBurg', 'Doug@game.ru', 3, true, '81dc9bdb52d04dc20036dbd8313ed055', '81dc9bdb52d04dc20036dbd8313ed055');

INSERT INTO `politician`.`users_history` (`id`, `stage`, `score`, `id_user`, `msg`) VALUES
('1', '1', 7, '3', 'заказ изменен'),
('2', '2', 9, '3', 'заказ изменен'),
('3', '3', 5, '3', 'заказ изменен'),
('4', '1', 6, '4', 'заказ изменен'),
('5', '2', 7, '4', 'заказ изменен'),
('6', '1', 8, '5', 'заказ изменен'),
('7', '2', 4, '5', 'заказ изменен'),
('8', '3', 3, '5', 'заказ изменен'),
('9', '1', 4, '6', 'заказ изменен'),
('10', '2', 5, '6', 'заказ изменен'),
('11', '3', 6, '6', 'заказ изменен'),
('12', '4', 4, '6', 'заказ изменен');

-- SELECT t.`id`, t.`id_hasp`,  g.`name` AS `группа` ,  t.`address` , t.`name`, 
-- p.id AS `id параметра`, p.name AS `имя параметра`, p.path AS `путь параметра` ,tp.value AS `значение параметра`, 
-- tp.last_edit_date, tp.save_date
-- FROM terminal_archive.terminals AS t
-- LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal
-- LEFT JOIN terminal_archive.groups AS g ON tg.id_group = g.id
-- LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
-- LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
-- /*WHERE tp.save_date < tp.last_edit_date*/
-- ORDER BY t.id desc;

-- SELECT o.`id`, s.name AS `состояние`,  t.`name` AS `терминал` ,  `RNN` , d.description AS `доп. параметр`, od.value AS `значение`,
-- f.`name` AS `топливо` , p.`name` AS `оплата` , o.id_pump AS `колонка`,  
-- `pre_price` ,  `price` ,  `pre_quantity` ,  `quantity` ,  `pre_summ` ,  `summ` FROM terminal_archive.orders AS o
-- LEFT JOIN terminal_archive.order_fuels AS f ON o.id_fuel = f.id
-- LEFT JOIN terminal_archive.order_payment_types AS p ON o.id_payment = p.id
-- LEFT JOIN terminal_archive.terminals AS t ON o.id_terminal = t.id
-- LEFT JOIN terminal_archive.order_states AS s ON o.id_state = s.id
-- LEFT JOIN terminal_archive.order_details AS od ON o.id = od.id_order
-- LEFT JOIN terminal_archive.details AS d ON od.id_detail = d.id
-- /*WHERE t.id = 1*/
-- ORDER BY o.id desc
-- LIMIT 20;

-- SELECT t.`id`, t.`id_hasp`,  g.`name` AS `группа` ,  t.`address` , t.`name`, 
-- p.id AS `id параметра`, p.name AS `имя параметра` ,p.path AS `путь параметра`, tp.value AS `значение параметра`, 
-- tp.last_edit_date, tp.save_date
-- FROM terminal_archive.terminals AS t
-- LEFT JOIN terminal_archive.terminal_groups AS tg ON t.id = tg.id_terminal
-- LEFT JOIN terminal_archive.groups AS g ON tg.id_group = g.id
-- LEFT JOIN terminal_archive.terminal_parameters AS tp ON t.id = tp.id_terminal
-- LEFT JOIN terminal_archive.parameters AS p ON tp.id_parameter = p.id
-- /*WHERE tp.save_date < tp.last_edit_date AND t.hasp_id='1'*/
-- ORDER BY t.id asc;  
